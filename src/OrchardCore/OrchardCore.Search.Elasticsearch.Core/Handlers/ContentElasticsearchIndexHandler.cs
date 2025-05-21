using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.Elasticsearch.Core;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.Elasticsearch.Handlers;

public sealed class ContentElasticsearchIndexHandler : ElasticsearchIndexSettingsHandlerBase
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IElasticsearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ElasticsearchIndexManager _indexManager;
    private readonly ILogger _logger;

    private readonly IStringLocalizer S;

    public ContentElasticsearchIndexHandler(
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IEnumerable<IElasticsearchFieldIndexEvents> fieldIndexEvents,
        ElasticsearchIndexManager indexManager,
        ILogger<ContentElasticsearchIndexHandler> logger,
        IStringLocalizer<ContentElasticsearchIndexHandler> stringLocalizer)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _indexManager = indexManager;
        _logger = logger;
        S = stringLocalizer;
    }

    public override Task CreatingAsync(ElasticsearchIndexSettingsCreateContext context)
        => SetMappingAsync(context.Settings);

    public override Task UpdatingAsync(ElasticsearchIndexSettingsUpdateContext context)
        => SetMappingAsync(context.Settings);

    public override Task InitializingAsync(ElasticsearchIndexSettingsInitializingContext context)
        => PopulateAsync(context.Settings, context.Data);

    private static Task PopulateAsync(ElasticIndexSettings settings, JsonNode data)
    {
        if (!CanHandle(settings))
        {
            return Task.CompletedTask;
        }

        var metadata = settings.As<ContentIndexMetadata>();

        var indexLatest = data[nameof(metadata.IndexLatest)]?.GetValue<bool>();

        if (indexLatest.HasValue)
        {
            metadata.IndexLatest = indexLatest.Value;
        }

        var storeSourceData = data[nameof(metadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            metadata.StoreSourceData = storeSourceData.Value;
        }

        var culture = data[nameof(metadata.Culture)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(culture))
        {
            metadata.Culture = culture;
        }

        var indexContentTypes = data[nameof(metadata.IndexedContentTypes)]?.AsArray();

        if (indexContentTypes is not null)
        {
            var items = new HashSet<string>();

            foreach (var indexContentType in indexContentTypes)
            {
                var value = indexContentType.GetValue<string>();

                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(value);
                }
            }

            metadata.IndexedContentTypes = items.ToArray();
        }

        settings.Put(metadata);

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(ElasticsearchIndexSettingsValidatingContext context)
    {
        if (!CanHandle(context.Settings))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Settings.As<ContentIndexMetadata>();

        if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
        {
            context.Result.Fail(new ValidationResult(S["At least one content type must be selected."]));
        }

        return Task.CompletedTask;
    }

    public override async Task ResetAsync(ElasticsearchIndexSettingsResetContext context)
    {
        if (!CanHandle(context.Settings))
        {
            return;
        }

        await _indexManager.SetLastTaskIdAsync(context.Settings.IndexName, 0);
    }

    public override Task SynchronizedAsync(ElasticsearchIndexSettingsSynchronizedContext context)
    {
        if (!CanHandle(context.Settings))
        {
            return Task.CompletedTask;
        }

        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-elasticsearch", context.Settings.IndexName, async (scope, indexName) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<ElasticsearchContentIndexingService>();
            await indexingService.ProcessContentItemsAsync(indexName);
        });
    }

    public override Task ExportingAsync(ElasticsearchIndexSettingsExportingContext context)
    {
        if (!CanHandle(context.Settings))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Settings.As<ContentIndexMetadata>();

        context.Data["IndexLatest"] = metadata.IndexLatest;
        context.Data["StoreSourceData"] = metadata.StoreSourceData;
        context.Data["Culture"] = metadata.Culture;

        var jsonArray = new JsonArray();

        foreach (var indexedContentType in metadata.IndexedContentTypes)
        {
            jsonArray.Add(indexedContentType);
        }

        context.Data["IndexedContentTypes"] = jsonArray;

        return Task.CompletedTask;
    }

    private async Task SetMappingAsync(ElasticIndexSettings settings)
    {
        if (!CanHandle(settings))
        {
            return;
        }

        var metadata = settings.As<ContentIndexMetadata>();

        foreach (var contentType in metadata.IndexedContentTypes ?? [])
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            var index = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(index, contentItem, [contentType], new ElasticContentIndexSettings());
            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            await AddIndexMappingAsync(settings.IndexMappings, IndexingConstants.ContentItemIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), settings);
            await AddIndexMappingAsync(settings.IndexMappings, IndexingConstants.ContentItemVersionIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemVersionIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), settings);

            foreach (var entry in index.Entries)
            {
                if (ElasticsearchIndexNameService.ToSafeIndexName(entry.Name) != entry.Name)
                {
                    continue;
                }

                await AddIndexMappingAsync(settings.IndexMappings, entry.Name, entry, settings);
            }
        }
    }

    private async Task AddIndexMappingAsync(TypeMapping mapping, string safeFieldName, DocumentIndexEntry entry, ElasticIndexSettings settings)
    {
        var indexMap = new ElasticsearchIndexMap(safeFieldName, entry.Type, entry.Options)
        {
            IndexingKey = entry.Name,
        };

        var context = new SearchIndexDefinition(mapping, entry, settings);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappingAsync(ctx), context, _logger);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappedAsync(ctx), context, _logger);
    }
}
