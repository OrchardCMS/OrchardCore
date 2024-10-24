using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// This class provides services to update all the Elasticsearch indices.
/// </summary>
public class ElasticIndexingService
{
    private const int BatchSize = 100;

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly ElasticIndexManager _indexManager;
    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly ElasticConnectionOptions _elasticConnectionOptions;
    private readonly ISiteService _siteService;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IStore _store;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILogger _logger;

    public ElasticIndexingService(
        IShellHost shellHost,
        ShellSettings shellSettings,
        ElasticIndexSettingsService elasticIndexSettingsService,
        ElasticIndexManager indexManager,
        IIndexingTaskManager indexingTaskManager,
        IOptions<ElasticConnectionOptions> elasticConnectionOptions,
        ISiteService siteService,
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IStore store,
        IContentDefinitionManager contentDefinitionManager,
        ILogger<ElasticIndexingService> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _indexManager = indexManager;
        _indexingTaskManager = indexingTaskManager;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _siteService = siteService;
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _store = store;
        _contentDefinitionManager = contentDefinitionManager;
        _logger = logger;
    }

    public async Task ProcessContentItemsAsync(params string[] indexNames)
    {
        if (!_elasticConnectionOptions.FileConfigurationExists())
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
            var taskId = await _indexManager.GetLastTaskId(indexSetting.IndexName);
            lastTaskId = Math.Min(lastTaskId, taskId);
            allIndices.Add(indexSetting.IndexName, taskId);
        }

        IEnumerable<IndexingTask> tasks = [];
        var allContentTypes = indexSettingsList.SelectMany(x => x.IndexedContentTypes ?? []).Distinct().ToArray();
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

            if (indexSettingsList.Any(x => !x.IndexLatest))
            {
                var publishedContentItems = await readOnlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(allContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds)).ListAsync();
                allPublished = publishedContentItems.DistinctBy(x => x.ContentItemId)
                .ToDictionary(k => k.ContentItemId);
            }

            if (indexSettingsList.Any(x => x.IndexLatest))
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

            var needPublished = indexSettingsList.FirstOrDefault(x => !x.IndexLatest) != null;

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

                    var context = !settings.IndexLatest ? publishedIndexContext : latestIndexContext;

                    // We index only if we actually found a content item in the database.
                    if (context == null)
                    {
                        // TODO purge these content items from IndexingTask table.
                        continue;
                    }

                    var cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(context.ContentItem);
                    var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                    var ignoreIndexedCulture = settings.Culture != "any" && culture != settings.Culture;

                    // Ignore if the content item content type or culture is not indexed in this index.
                    if (!settings.IndexedContentTypes.Contains(context.ContentItem.ContentType) || ignoreIndexedCulture)
                    {
                        continue;
                    }

                    updatedDocumentsByIndex[index.Key].Add(context.DocumentIndex);
                }
            }

            // Delete all the existing documents.
            foreach (var index in updatedDocumentsByIndex)
            {
                var deletedDocuments = updatedDocumentsByIndex[index.Key].Select(x => x.ContentItemId);
                await _indexManager.DeleteDocumentsAsync(index.Key, deletedDocuments);
            }

            // Submits all the new documents to the index.
            foreach (var index in updatedDocumentsByIndex)
            {
                await _indexManager.StoreDocumentsAsync(index.Key, updatedDocumentsByIndex[index.Key]);
            }

            // Update task ids.
            lastTaskId = tasks.Last().Id;

            foreach (var indexStatus in allIndices)
            {
                if (indexStatus.Value < lastTaskId)
                {
                    await _indexManager.SetLastTaskId(indexStatus.Key, lastTaskId);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new index.
    /// </summary>
    public async Task CreateIndexAsync(ElasticIndexSettings elasticIndexSettings)
    {
        await _elasticIndexSettingsService.UpdateIndexAsync(elasticIndexSettings);
        await RebuildIndexAsync(elasticIndexSettings);
    }

    /// <summary>
    /// Update an existing index.
    /// </summary>
    public Task UpdateIndexAsync(ElasticIndexSettings elasticIndexSettings)
        => _elasticIndexSettingsService.UpdateIndexAsync(elasticIndexSettings);

    /// <summary>
    /// Deletes permanently an index.
    /// </summary>
    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        // Delete the Elasticsearch Index first.
        var result = await _indexManager.DeleteIndex(indexName);

        if (result)
        {
            // Now delete it's setting.
            await _elasticIndexSettingsService.DeleteIndexAsync(indexName);
        }

        return result;
    }

    /// <summary>
    /// Restarts the indexing process from the beginning in order to update
    /// current content items. It doesn't delete existing entries from the index.
    /// </summary>
    public async Task ResetIndexAsync(string indexName)
    {
        await _indexManager.SetLastTaskId(indexName, 0);
    }

    /// <summary>
    /// Deletes and recreates the full index content.
    /// </summary>
    public async Task RebuildIndexAsync(ElasticIndexSettings elasticIndexSettings)
    {
        await _indexManager.DeleteIndex(elasticIndexSettings.IndexName);
        await _indexManager.CreateIndexAsync(elasticIndexSettings);
        await ResetIndexAsync(elasticIndexSettings.IndexName);
    }

    public async Task<ElasticSettings> GetElasticSettingsAsync()
        => await _siteService.GetSettingsAsync<ElasticSettings>() ?? new ElasticSettings();

    /// <summary>
    /// Synchronizes Elasticsearch content index settings with Lucene ones.
    /// </summary>
    public async Task SyncSettings()
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
