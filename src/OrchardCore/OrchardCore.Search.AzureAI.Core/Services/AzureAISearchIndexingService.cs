using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchIndexingService
{
    private const int _batchSize = 100;

    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly AzureAISearchIndexSettingsService _azureAISearchIndexSettingsService;
    private readonly ILogger _logger;

    public AzureAISearchIndexingService(
        IIndexingTaskManager indexingTaskManager,
        AzureAISearchIndexSettingsService azureAISearchIndexSettingsService,
        ILogger<AzureAISearchIndexingService> logger)
    {
        _indexingTaskManager = indexingTaskManager;
        _azureAISearchIndexSettingsService = azureAISearchIndexSettingsService;
        _logger = logger;
    }


    public async Task ProcessContentItemsAsync(string indexName = default)
    {
        var lastTaskId = long.MaxValue;
        var indexSettings = new List<AzureAISearchIndexSettings>();
        var indexesDocument = await _azureAISearchIndexSettingsService.LoadDocumentAsync();

        if (string.IsNullOrEmpty(indexName))
        {
            indexSettings = new List<AzureAISearchIndexSettings>(indexesDocument.IndexSettings.Values);
        }
        else
        {
            indexSettings = indexesDocument.IndexSettings.Where(x => x.Key == indexName)
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

        if (indexSettings.Count == 0)
        {
            return;
        }

        var batch = new List<IndexingTask>();

        while (batch.Count <= _batchSize)
        {
            // Create a scope for the content manager.
            await ShellScope.Current.UsingAsync(async scope =>
            {
                var indexDocumentManager = scope.ServiceProvider.GetRequiredService<AzureAIIndexDocumentManager>();

                // Load the next batch of tasks.
                batch = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, _batchSize)).ToList();

                if (batch.Count == 0)
                {
                    return;
                }

                var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                var indexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();

                // Pre-load all content items to prevent SELECT N+1.
                var updatedContentItemIds = batch
                    .Where(x => x.Type == IndexingTaskTypes.Update)
                    .Select(x => x.ContentItemId)
                    .ToArray();

                Dictionary<string, ContentItem> allPublished = null;
                Dictionary<string, ContentItem> allLatest = null;

                // Group all DocumentIndex by index to batch update them.
                var updatedDocumentsByIndex = indexSettings.ToDictionary(x => x.IndexName, b => new List<DocumentIndex>());

                var settingsByIndex = indexSettings.ToDictionary(x => x.IndexName, x => x);

                if (indexSettings.Any(x => !x.IndexLatest))
                {
                    var allLatestContentItems = await contentManager.GetAsync(updatedContentItemIds, VersionOptions.Published);
                    allPublished = allLatestContentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemVersionId);
                }

                if (indexSettings.Any(x => x.IndexLatest))
                {
                    var allLatestContentItems = await contentManager.GetAsync(updatedContentItemIds, VersionOptions.Latest);
                    allLatest = allLatestContentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemVersionId);
                }

                foreach (var task in batch)
                {
                    if (task.Type == IndexingTaskTypes.Update)
                    {
                        BuildIndexContext publishedIndexContext = null, latestIndexContext = null;

                        if (allPublished != null && allPublished.TryGetValue(task.ContentItemId, out var publishedContentItem))
                        {
                            publishedIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, publishedContentItem.ContentItemVersionId), publishedContentItem, [publishedContentItem.ContentType], new AzureAISearchContentIndexSettings());
                            await indexHandlers.InvokeAsync(x => x.BuildIndexAsync(publishedIndexContext), _logger);
                        }

                        if (allLatest != null && allLatest.TryGetValue(task.ContentItemId, out var latestContentItem))
                        {
                            latestIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, latestContentItem.ContentItemVersionId), latestContentItem, [latestContentItem.ContentType], new AzureAISearchContentIndexSettings());
                            await indexHandlers.InvokeAsync(x => x.BuildIndexAsync(latestIndexContext), _logger);
                        }

                        // Update the document from the index if its lastIndexId is smaller than the current task id.
                        foreach (var settings in indexSettings)
                        {
                            if (settings.GetLastTaskId() >= task.Id || !settingsByIndex.ContainsKey(settings.IndexName))
                            {
                                continue;
                            }

                            var context = !settings.IndexLatest ? publishedIndexContext : latestIndexContext;

                            // We index only if we actually found a content item in the database.
                            if (context == null)
                            {
                                // TODO purge these content items from IndexingTask table.
                                continue;
                            }

                            var cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(context.ContentItem);
                            var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                            var ignoreIndexedCulture = settings.Culture != "any" && culture != settings.Culture;

                            // Ignore if the content item content type or culture is not indexed in this index.
                            if (!settings.IndexedContentTypes.Contains(context.ContentItem.ContentType) || ignoreIndexedCulture)
                            {
                                continue;
                            }

                            updatedDocumentsByIndex[settings.IndexName].Add(context.DocumentIndex);
                        }
                    }
                }

                var resultTracker = new HashSet<string>();
                // Send all the new documents to the index.
                foreach (var index in updatedDocumentsByIndex)
                {
                    var settings = indexSettings.FirstOrDefault(x => x.IndexName == index.Key);

                    if (settings == null)
                    {
                        continue;
                    }

                    if (!await indexDocumentManager.MergeOrUploadDocumentsAsync(index.Key, updatedDocumentsByIndex[index.Key], settings))
                    {
                        // At this point we know something went wrong while trying update content items for this index.
                        resultTracker.Add(index.Key);

                        continue;
                    }

                    if (!resultTracker.Contains(index.Key))
                    {
                        // We know none of the previous batches failed to update this index.
                        settings.SetLastTaskId(batch.Last().Id);
                        await _azureAISearchIndexSettingsService.UpdateIndexAsync(settings);
                    }
                }

            }, activateShell: false);

            if (batch.Count == 0)
            {
                break;
            }
        }
    }
}
