using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// This class provides services to update all the Elasticsearch indices.
/// </summary>
public class ElasticsearchContentIndexingService
{
    private const int BatchSize = 100;

    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;
    private readonly ElasticsearchIndexManager _indexManager;
    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IStore _store;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILogger _logger;

    public ElasticsearchContentIndexingService(
        ElasticsearchIndexSettingsService elasticIndexSettingsService,
        ElasticsearchIndexManager indexManager,
        IIndexingTaskManager indexingTaskManager,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IStore store,
        IContentDefinitionManager contentDefinitionManager,
        ILogger<ElasticsearchContentIndexingService> logger)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _indexManager = indexManager;
        _indexingTaskManager = indexingTaskManager;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _store = store;
        _contentDefinitionManager = contentDefinitionManager;
        _logger = logger;
    }

    public async Task ProcessContentItemsAsync(params string[] indexNames)
    {
        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return;
        }

        var indexSettingsList = await _elasticIndexSettingsService.GetSettingsAsync();

        if (indexNames != null && indexNames.Length > 0)
        {
            indexSettingsList = indexSettingsList
                .Where(x => indexNames.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));
        }

        if (!indexSettingsList.Any())
        {
            return;
        }

        var allIndices = new Dictionary<string, long>();
        var lastTaskId = long.MaxValue;

        // Find the lowest task id to process.
        foreach (var indexSetting in indexSettingsList)
        {
            var taskId = await _indexManager.GetLastTaskIdAsync(indexSetting.IndexName);
            lastTaskId = Math.Min(lastTaskId, taskId);
            allIndices.Add(indexSetting.IndexName, taskId);
        }

        IEnumerable<IndexingTask> tasks = [];
        var allContentTypes = indexSettingsList.SelectMany(x => x.As<ContentIndexMetadata>().IndexedContentTypes ?? []).Distinct().ToArray();
        var readOnlySession = _store.CreateSession(withTracking: false);

        while (true)
        {
            // Load the next batch of tasks.
            tasks = await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, BatchSize);

            if (!tasks.Any())
            {
                break;
            }

            var updatedContentItemIds = tasks
                .Where(x => x.Type == IndexingTaskTypes.Update)
                .Select(x => x.ContentItemId)
                .ToList();

            Dictionary<string, ContentItem> allPublished = null;
            Dictionary<string, ContentItem> allLatest = null;

            var settingsByIndex = indexSettingsList.ToDictionary(x => x.IndexName);


            if (indexSettingsList.Any(x => !x.As<ContentIndexMetadata>().IndexLatest))
            {
                var publishedContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds)).ListAsync();
                allPublished = publishedContentItems.DistinctBy(x => x.ContentItemId)
                .ToDictionary(k => k.ContentItemId);
            }

            if (indexSettingsList.Any(x => x.As<ContentIndexMetadata>().IndexLatest))
            {
                var latestContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds)).ListAsync();
                allLatest = latestContentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
            }

            // Group all DocumentIndex by index to batch update them.
            var updatedDocumentsByIndex = new Dictionary<string, List<DocumentIndex>>();

            foreach (var index in allIndices)
            {
                updatedDocumentsByIndex[index.Key] = [];
            }

            var needPublished = indexSettingsList.Any(x => !x.As<ContentIndexMetadata>().IndexLatest);

            foreach (var task in tasks)
            {
                if (task.Type != IndexingTaskTypes.Update)
                {
                    continue;
                }

                BuildIndexContext publishedIndexContext = null, latestIndexContext = null;

                if (allPublished != null && allPublished.TryGetValue(task.ContentItemId, out var publishedContentItem))
                {
                    publishedIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, publishedContentItem.ContentItemVersionId), publishedContentItem, [publishedContentItem.ContentType], new ElasticContentIndexSettings());
                    await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(publishedIndexContext), _logger);
                }

                if (allLatest != null && allLatest.TryGetValue(task.ContentItemId, out var latestContentItem))
                {
                    latestIndexContext = new BuildIndexContext(new DocumentIndex(task.ContentItemId, latestContentItem.ContentItemVersionId), latestContentItem, [latestContentItem.ContentType], new ElasticContentIndexSettings());
                    await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(latestIndexContext), _logger);
                }

                if (publishedIndexContext == null && latestIndexContext == null)
                {
                    continue;
                }

                // Update the document from the index if its lastIndexId is smaller than the current task id.
                foreach (var index in allIndices)
                {
                    if (index.Value >= task.Id || !settingsByIndex.TryGetValue(index.Key, out var settings))
                    {
                        continue;
                    }

                    var metadata = settings.As<ContentIndexMetadata>();

                    var context = !metadata.IndexLatest ? publishedIndexContext : latestIndexContext;

                    // We index only if we actually found a content item in the database.
                    if (context == null)
                    {
                        // TODO purge these content items from IndexingTask table.
                        continue;
                    }

                    var cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(context.ContentItem);
                    var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                    var ignoreIndexedCulture = metadata.Culture != "any" && culture != metadata.Culture;

                    // Ignore if the content item content type or culture is not indexed in this index.
                    if (!metadata.IndexedContentTypes.Contains(context.ContentItem.ContentType) || ignoreIndexedCulture)
                    {
                        continue;
                    }

                    updatedDocumentsByIndex[index.Key].Add(context.DocumentIndex);
                }
            }

            // Delete all the existing documents.
            foreach (var index in updatedDocumentsByIndex)
            {
                if (!settingsByIndex.TryGetValue(index.Key, out var settings))
                {
                    continue;
                }

                var deletedDocuments = updatedDocumentsByIndex[index.Key].Select(x => x.ContentItemId);
                await _indexManager.DeleteDocumentsAsync(settings.IndexName, deletedDocuments);
            }

            // Submits all the new documents to the index.
            foreach (var index in updatedDocumentsByIndex)
            {
                if (!settingsByIndex.TryGetValue(index.Key, out var settings))
                {
                    continue;
                }

                await _indexManager.StoreDocumentsAsync(settings, updatedDocumentsByIndex[index.Key]);
            }

            // Update task ids.
            lastTaskId = tasks.Last().Id;

            foreach (var indexStatus in allIndices)
            {
                if (indexStatus.Value < lastTaskId && settingsByIndex.TryGetValue(indexStatus.Key, out var settings))
                {
                    await _indexManager.SetLastTaskIdAsync(settings.IndexName, lastTaskId);
                }
            }
        }
    }

    /// <summary>
    /// Synchronizes Elasticsearch content index settings with Lucene ones.
    /// </summary>
    public async Task SyncSettingsAsync()
    {
        var contentTypeDefinitions = await _contentDefinitionManager.LoadTypeDefinitionsAsync();

        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            foreach (var partDefinition in contentTypeDefinition.Parts)
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    if (partDefinition.Settings.TryGetPropertyValue("LuceneContentIndexSettings", out var existingPartSettings))
                    {
                        var included = existingPartSettings["Included"];

                        if (included is not null && (bool)included)
                        {
                            partDefinition.Settings[nameof(ElasticContentIndexSettings)] = JNode.FromObject(existingPartSettings.ToObject<ElasticContentIndexSettings>());
                        }
                    }
                });
            }
        }

        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();

        foreach (var partDefinition in partDefinitions)
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
            {
                if (partDefinition.Settings.TryGetPropertyValue("LuceneContentIndexSettings", out var existingPartSettings))
                {
                    var included = existingPartSettings["Included"];

                    if (included != null && (bool)included)
                    {
                        partDefinition.Settings[nameof(ElasticContentIndexSettings)] = JNode.FromObject(existingPartSettings.ToObject<ElasticContentIndexSettings>());
                    }
                }

                foreach (var fieldDefinition in partDefinition.Fields)
                {
                    if (fieldDefinition.Settings.TryGetPropertyValue("LuceneContentIndexSettings", out var existingFieldSettings))
                    {
                        var included = existingFieldSettings["Included"];

                        if (included != null && (bool)included)
                        {
                            fieldDefinition.Settings[nameof(ElasticContentIndexSettings)] = JNode.FromObject(existingFieldSettings.ToObject<ElasticContentIndexSettings>());
                        }
                    }
                }
            });
        }
    }
}
