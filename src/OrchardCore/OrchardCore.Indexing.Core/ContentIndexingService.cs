using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Indexing.Core;

public sealed class ContentIndexingService
{
    private const int _batchSize = 100;

    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly IIndexProfileStore _indexStore;
    private readonly IStore _store;
    private readonly IContentManager _contentManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IDocumentIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly HashSet<string> _inProgressIndexes = [];

    public ContentIndexingService(
        IIndexingTaskManager indexingTaskManager,
        IIndexProfileStore indexStore,
        IStore store,
        IContentManager contentManager,
        IServiceProvider serviceProvider,
        IEnumerable<IDocumentIndexHandler> contentItemIndexHandlers,
        ILogger<ContentIndexingService> logger)
    {
        _indexingTaskManager = indexingTaskManager;
        _indexStore = indexStore;
        _store = store;
        _contentManager = contentManager;
        _serviceProvider = serviceProvider;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _logger = logger;
    }

    public async Task ProcessContentItemsForAllIndexesAsync()
    {
        await ProcessContentItemsAsync((await _indexStore.GetByTypeAsync(IndexingConstants.ContentsIndexSource)));
    }

    public async Task ProcessContentItemsAsync(IEnumerable<IndexProfile> indexes)
    {
        ArgumentNullException.ThrowIfNull(indexes);

        if (!indexes.Any())
        {
            return;
        }

        // This service could be called multiple times during the same request,
        // so we need to ensure that we only process each index once at a time.
        // This is not guaranteed to be thread-safe, but it should be sufficient for the current use case.
        await _semaphore.WaitAsync();

        var indexableIndexes = new List<IndexProfile>();

        var tracker = new Dictionary<string, IndexingPosition>();

        var documentIndexManagers = new Dictionary<string, IDocumentIndexManager>();

        var lastTaskId = long.MaxValue;

        try
        {
            // Find the lowest task id to process.
            foreach (var index in indexes)
            {
                if (index.Type != IndexingConstants.ContentsIndexSource)
                {
                    // Skip indexes that are not content indexes.
                    continue;
                }

                if (!_inProgressIndexes.Add(index.Id))
                {
                    // If the index is already being processed, skip it.
                    continue;
                }

                indexableIndexes.Add(index);

                if (!documentIndexManagers.TryGetValue(index.ProviderName, out var documentIndexManager))
                {
                    documentIndexManager = _serviceProvider.GetRequiredKeyedService<IDocumentIndexManager>(index.ProviderName);
                    documentIndexManagers.Add(index.ProviderName, documentIndexManager);
                }

                var taskId = await documentIndexManager.GetLastTaskIdAsync(index);
                lastTaskId = Math.Min(lastTaskId, taskId);
                tracker.Add(index.Id, new IndexingPosition
                {
                    Index = index,
                    LastTaskId = taskId,
                });
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (indexableIndexes.Count == 0)
        {
            return;
        }

        var tasks = new List<IndexingTask>();

        var latestContentTypes = new HashSet<string>();
        var publishedContentTypes = new HashSet<string>();

        foreach (var index in indexableIndexes)
        {
            var metadata = index.As<ContentIndexMetadata>();

            if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
            {
                continue;
            }

            if (metadata.IndexLatest)
            {
                foreach (var contentType in metadata.IndexedContentTypes)
                {
                    latestContentTypes.Add(contentType);
                }
            }
            else
            {
                foreach (var contentType in metadata.IndexedContentTypes)
                {
                    publishedContentTypes.Add(contentType);
                }
            }
        }

        var readOnlySession = _store.CreateSession(withTracking: false);

        while (tasks.Count <= _batchSize)
        {
            // Load the next batch of tasks.
            tasks = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, _batchSize, IndexingConstants.ContentsIndexSource)).ToList();

            if (tasks.Count == 0)
            {
                break;
            }

            var updatedContentItemIds = tasks
                .Where(x => x.Type == IndexingTaskTypes.Update)
                .Select(x => x.RecordId)
                .ToArray();

            var publishedContentItems = new Dictionary<string, ContentItem>();
            var latestContentItems = new Dictionary<string, ContentItem>();

            // Group all DocumentIndex by index to batch update them.
            var updatedDocumentsByIndex = indexableIndexes.ToDictionary(x => x.Id, b => new List<DocumentIndex>());

            if (publishedContentTypes.Count > 0)
            {
                var contentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(publishedContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                    .ListAsync();

                publishedContentItems = contentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
            }

            if (latestContentTypes.Count > 0)
            {
                var contentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.ContentType.IsIn(latestContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                    .ListAsync();

                latestContentItems = contentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
            }

            var cultureAspects = new Dictionary<string, CultureAspect>();

            // Update the document from the index if its lastIndexId is smaller than the current task id.
            foreach (var index in indexableIndexes)
            {
                if (!tracker.TryGetValue(index.Id, out var indexPosition))
                {
                    continue;
                }

                var metadata = index.As<ContentIndexMetadata>();
                var anyCulture = string.IsNullOrEmpty(metadata.Culture) || metadata.Culture == "any";

                foreach (var task in tasks)
                {
                    if (task.Id < indexPosition.LastTaskId)
                    {
                        continue;
                    }

                    if (task.Type != IndexingTaskTypes.Update)
                    {
                        continue;
                    }

                    // Handle the updated documents.
                    var indexManager = documentIndexManagers[index.ProviderName];

                    ContentItem contentItem = null;

                    if (metadata.IndexLatest && latestContentItems.TryGetValue(task.RecordId, out var latestContentItem) && metadata.IndexedContentTypes.Contains(latestContentItem.ContentType))
                    {
                        contentItem = latestContentItem;
                    }

                    if (contentItem is null && publishedContentItems.TryGetValue(task.RecordId, out var publishedContentItem) && metadata.IndexedContentTypes.Contains(publishedContentItem.ContentType))
                    {
                        contentItem = publishedContentItem;
                    }

                    // We index only if we actually found a content item in the database.
                    if (contentItem is null)
                    {
                        continue;
                    }

                    var buildIndexContext = new BuildDocumentIndexContext(new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId), contentItem, [contentItem.ContentType], indexManager.GetContentIndexSettings());

                    await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

                    // Ignore if the culture is not indexed in this index.
                    if (!anyCulture)
                    {
                        if (!cultureAspects.TryGetValue(contentItem.ContentItemVersionId ?? contentItem.ContentItemId, out var cultureAspect) && buildIndexContext.Record is ContentItem record)
                        {
                            cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(record);
                            cultureAspects[record.ContentItemVersionId ?? record.ContentItemId] = cultureAspect;
                        }

                        if (cultureAspect.Culture?.Name != metadata.Culture)
                        {
                            continue;
                        }
                    }

                    updatedDocumentsByIndex[index.Id].Add(buildIndexContext.DocumentIndex);
                }
            }

            lastTaskId = tasks.Last().Id;

            foreach (var indexEntry in updatedDocumentsByIndex)
            {
                if (indexEntry.Value.Count == 0)
                {
                    continue;
                }

                var index = tracker[indexEntry.Key].Index;
                var indexManager = documentIndexManagers[index.ProviderName];

                // Delete all the deleted documents from the index.
                var deletedDocumentIds = indexEntry.Value.Select(x => x.Id);

                await indexManager.DeleteDocumentsAsync(index, deletedDocumentIds);

                // Upload documents to the index.
                if (await indexManager.AddOrUpdateDocumentsAsync(index, indexEntry.Value))
                {
                    // We know none of the previous batches failed to update this index.
                    await indexManager.SetLastTaskIdAsync(index, lastTaskId);
                }
            }
        }

        foreach (var index in indexableIndexes)
        {
            _inProgressIndexes.Remove(index.Id);
        }
    }

    private sealed class IndexingPosition
    {
        public IndexProfile Index { get; set; }

        public long LastTaskId { get; set; }
    }
}
