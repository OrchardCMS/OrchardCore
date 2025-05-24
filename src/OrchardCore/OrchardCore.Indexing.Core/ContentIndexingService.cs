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
    private readonly IIndexEntityStore _indexStore;
    private readonly IStore _store;
    private readonly IContentManager _contentManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;

    public ContentIndexingService(
        IIndexingTaskManager indexingTaskManager,
        IIndexEntityStore indexStore,
        IStore store,
        IContentManager contentManager,
        IServiceProvider serviceProvider,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
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
        await ProcessContentItemsAsync(await _indexStore.GetAllAsync());
    }

    public async Task ProcessContentItemsAsync(IEnumerable<IndexEntity> indexes)
    {
        ArgumentNullException.ThrowIfNull(indexes);

        if (!indexes.Any())
        {
            return;
        }

        var tracker = new Dictionary<string, IndexingPosition>();

        var indexManagers = new Dictionary<string, IIndexDocumentManager>();

        var lastTaskId = long.MaxValue;

        // Find the lowest task id to process.
        foreach (var index in indexes)
        {
            if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
            {
                indexManager = _serviceProvider.GetRequiredKeyedService<IIndexDocumentManager>(index.ProviderName);
                indexManagers.Add(index.ProviderName, indexManager);
            }

            var taskId = await indexManager.GetLastTaskIdAsync(index);
            lastTaskId = Math.Min(lastTaskId, taskId);
            tracker.Add(index.Id, new IndexingPosition
            {
                Index = index,
                LastTaskId = taskId,
            });
        }

        var tasks = new List<IndexingTask>();

        var allContentTypes = indexes.SelectMany(x => x.As<ContentIndexMetadata>().IndexedContentTypes ?? []).Distinct().ToArray();
        var readOnlySession = _store.CreateSession(withTracking: false);

        while (tasks.Count <= _batchSize)
        {
            // Load the next batch of tasks.
            tasks = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, _batchSize)).ToList();

            if (tasks.Count == 0)
            {
                break;
            }

            var updatedContentItemIds = tasks
                .Where(x => x.Type == IndexingTaskTypes.Update)
                .Select(x => x.ContentItemId)
                .ToArray();

            Dictionary<string, ContentItem> allPublished = null;
            Dictionary<string, ContentItem> allLatest = null;

            // Group all DocumentIndex by index to batch update them.
            var updatedDocumentsByIndex = indexes.ToDictionary(x => x.Id, b => new List<DocumentIndexBase>());

            if (indexes.Any(x => !x.As<ContentIndexMetadata>().IndexLatest))
            {
                var publishedContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                    .ListAsync();

                allPublished = publishedContentItems.DistinctBy(x => x.ContentItemId)
                .ToDictionary(k => k.ContentItemId);
            }

            if (indexes.Any(x => x.As<ContentIndexMetadata>().IndexLatest))
            {
                var latestContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                    .ListAsync();

                allLatest = latestContentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
            }

            foreach (var task in tasks)
            {
                if (task.Type == IndexingTaskTypes.Update)
                {
                    // Update the document from the index if its lastIndexId is smaller than the current task id.
                    foreach (var index in indexes)
                    {
                        if (!tracker.TryGetValue(index.Id, out var indexPosition))
                        {
                            continue;
                        }

                        if (indexPosition.LastTaskId >= task.Id)
                        {
                            continue;
                        }

                        var metadata = index.As<ContentIndexMetadata>();

                        var indexManager = indexManagers[index.ProviderName];

                        BuildIndexContext buildIndexContext = null;

                        if (metadata.IndexLatest && allLatest.TryGetValue(task.ContentItemId, out var latestContentItem))
                        {
                            buildIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, latestContentItem.ContentItemVersionId), latestContentItem, [latestContentItem.ContentType], indexManager.GetContentIndexSettings());
                        }

                        if (buildIndexContext is null && allPublished.TryGetValue(task.ContentItemId, out var publishedContentItem))
                        {
                            buildIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, publishedContentItem.ContentItemVersionId), publishedContentItem, [publishedContentItem.ContentType], indexManager.GetContentIndexSettings());
                        }

                        // We index only if we actually found a content item in the database.
                        if (buildIndexContext == null)
                        {
                            continue;
                        }

                        // Ignore if the content item content type is not indexed in this index.
                        if (!metadata.IndexedContentTypes.Contains(buildIndexContext.ContentItem.ContentType))
                        {
                            continue;
                        }

                        await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

                        // Ignore if the culture is not indexed in this index.
                        var cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(buildIndexContext.ContentItem);
                        var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                        var ignoreIndexedCulture = metadata.Culture != "any" && culture != metadata.Culture;

                        if (ignoreIndexedCulture)
                        {
                            continue;
                        }

                        updatedDocumentsByIndex[index.Id].Add(buildIndexContext.DocumentIndex);
                    }
                }
            }

            lastTaskId = tasks.Last().Id;

            var resultTracker = new HashSet<string>();
            // Send all the new documents to the index.
            foreach (var indexEntry in updatedDocumentsByIndex)
            {
                if (indexEntry.Value.Count == 0)
                {
                    continue;
                }

                var index = tracker[indexEntry.Key].Index;
                var indexManager = indexManagers[index.ProviderName];

                if (!await indexManager.MergeOrUploadDocumentsAsync(indexEntry.Key, indexEntry.Value, index))
                {
                    // At this point we know something went wrong while trying update content items for this index.
                    resultTracker.Add(indexEntry.Key);

                    continue;
                }

                if (!resultTracker.Contains(indexEntry.Key))
                {
                    // We know none of the previous batches failed to update this index.
                    await indexManager.SetLastTaskIdAsync(index, lastTaskId);
                }
            }
        }
    }

    private sealed class IndexingPosition
    {
        public IndexEntity Index { get; set; }

        public long LastTaskId { get; set; }
    }
}
