using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchIndexingService
{
    private const int _batchSize = 100;

    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly AzureAISearchIndexSettingsService _azureAISearchIndexSettingsService;
    private readonly AzureAIIndexDocumentManager _indexDocumentManager;
    private readonly IStore _store;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;

    public AzureAISearchIndexingService(
        IIndexingTaskManager indexingTaskManager,
        AzureAISearchIndexSettingsService azureAISearchIndexSettingsService,
        AzureAIIndexDocumentManager indexDocumentManager,
        IStore store,
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        ILogger<AzureAISearchIndexingService> logger)
    {
        _indexingTaskManager = indexingTaskManager;
        _azureAISearchIndexSettingsService = azureAISearchIndexSettingsService;
        _indexDocumentManager = indexDocumentManager;
        _store = store;
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _logger = logger;
    }

    public async Task ProcessContentItemsAsync(params string[] indexNames)
    {
        var lastTaskId = long.MaxValue;
        var indexSettings = new List<AzureAISearchIndexSettings>();
        var indexesDocument = await _azureAISearchIndexSettingsService.LoadDocumentAsync();

        if (indexNames == null || indexNames.Length == 0)
        {
            indexSettings = new List<AzureAISearchIndexSettings>(indexesDocument.IndexSettings.Values);
        }
        else
        {
            indexSettings = indexesDocument.IndexSettings.Where(x => indexNames.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
                .Select(x => x.Value)
                .ToList();
        }

        if (indexSettings.Count == 0)
        {
            return;
        }

        // Find the lowest task id to process.
        foreach (var indexSetting in indexSettings)
        {
            var taskId = indexSetting.GetLastTaskId();
            lastTaskId = Math.Min(lastTaskId, taskId);
        }

        var tasks = new List<IndexingTask>();

        var allContentTypes = indexSettings.SelectMany(x => x.IndexedContentTypes ?? []).Distinct().ToArray();
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
            var updatedDocumentsByIndex = indexSettings.ToDictionary(x => x.IndexName, b => new List<DocumentIndexBase>());

            var settingsByIndex = indexSettings.ToDictionary(x => x.IndexName);

            if (indexSettings.Any(x => !x.IndexLatest))
            {
                var publishedContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds)).ListAsync();
                allPublished = publishedContentItems.DistinctBy(x => x.ContentItemId)
                .ToDictionary(k => k.ContentItemId);
            }

            if (indexSettings.Any(x => x.IndexLatest))
            {
                var latestContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds)).ListAsync();
                allLatest = latestContentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
            }

            foreach (var task in tasks)
            {
                if (task.Type == IndexingTaskTypes.Update)
                {
                    BuildIndexContext publishedIndexContext = null, latestIndexContext = null;

                    if (allPublished != null && allPublished.TryGetValue(task.ContentItemId, out var publishedContentItem))
                    {
                        publishedIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, publishedContentItem.ContentItemVersionId), publishedContentItem, [publishedContentItem.ContentType], new AzureAISearchContentIndexSettings());
                        await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(publishedIndexContext), _logger);
                    }

                    if (allLatest != null && allLatest.TryGetValue(task.ContentItemId, out var latestContentItem))
                    {
                        latestIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, latestContentItem.ContentItemVersionId), latestContentItem, [latestContentItem.ContentType], new AzureAISearchContentIndexSettings());
                        await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(latestIndexContext), _logger);
                    }

                    if (publishedIndexContext == null && latestIndexContext == null)
                    {
                        continue;
                    }

                    // Update the document from the index if its lastIndexId is smaller than the current task id.
                    foreach (var settings in indexSettings)
                    {
                        if (settings.GetLastTaskId() >= task.Id)
                        {
                            continue;
                        }

                        var context = !settings.IndexLatest ? publishedIndexContext : latestIndexContext;

                        // We index only if we actually found a content item in the database.
                        if (context == null)
                        {
                            continue;
                        }

                        // Ignore if the content item content type is not indexed in this index.
                        if (!settings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                        {
                            continue;
                        }

                        // Ignore if the culture is not indexed in this index.
                        var cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(context.ContentItem);
                        var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                        var ignoreIndexedCulture = settings.Culture != "any" && culture != settings.Culture;

                        if (ignoreIndexedCulture)
                        {
                            continue;
                        }

                        updatedDocumentsByIndex[settings.IndexName].Add(context.DocumentIndex);
                    }
                }
            }

            lastTaskId = tasks.Last().Id;

            var resultTracker = new HashSet<string>();
            // Send all the new documents to the index.
            foreach (var index in updatedDocumentsByIndex)
            {
                if (index.Value.Count == 0)
                {
                    continue;
                }

                var settings = indexSettings.FirstOrDefault(x => x.IndexName == index.Key);

                if (settings == null)
                {
                    continue;
                }

                if (!await _indexDocumentManager.MergeOrUploadDocumentsAsync(index.Key, updatedDocumentsByIndex[index.Key], settings))
                {
                    // At this point we know something went wrong while trying update content items for this index.
                    resultTracker.Add(index.Key);

                    continue;
                }

                if (!resultTracker.Contains(index.Key))
                {
                    // We know none of the previous batches failed to update this index.
                    settings.SetLastTaskId(lastTaskId);
                    await _azureAISearchIndexSettingsService.UpdateAsync(settings);
                }
            }
        }
    }
}
