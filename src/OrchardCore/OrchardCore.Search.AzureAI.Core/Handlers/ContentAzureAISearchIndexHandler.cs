using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class ContentAzureAISearchIndexHandler : AzureAISearchIndexSettingsHandlerBase
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    private readonly IStringLocalizer S;

    public ContentAzureAISearchIndexHandler(
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IEnumerable<IAzureAISearchFieldIndexEvents> fieldIndexEvents,
        ILogger<ContentAzureAISearchIndexHandler> logger,
        IStringLocalizer<ContentAzureAISearchIndexHandler> stringLocalizer)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _logger = logger;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context)
        => PopulateAsync(context.Settings, context.Data);

    private static Task PopulateAsync(AzureAISearchIndexSettings settings, JsonNode data)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = settings.As<ContentIndexMetadata>();

        var indexLatest = data[nameof(ContentIndexMetadata.IndexLatest)]?.GetValue<bool>();

        if (indexLatest.HasValue)
        {
            metadata.IndexLatest = indexLatest.Value;
        }

        var culture = data[nameof(ContentIndexMetadata.Culture)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(culture))
        {
            metadata.Culture = culture;
        }

        var indexContentTypes = data[nameof(ContentIndexMetadata.IndexedContentTypes)]?.AsArray();

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

    public override Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
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

    public override Task ResetAsync(AzureAISearchIndexSettingsResetContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        context.Settings.SetLastTaskId(0);

        return Task.CompletedTask;
    }

    public override Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-azure-ai", context.Settings.IndexName, async (scope, indexName) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<AzureAISearchIndexingService>();
            await indexingService.ProcessContentItemsAsync(indexName);
        });
    }

    public override Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Settings.As<ContentIndexMetadata>();

        context.Data["IndexLatest"] = metadata.IndexLatest;
        context.Data["Culture"] = metadata.Culture;

        var jsonArray = new JsonArray();

        foreach (var indexedContentType in metadata.IndexedContentTypes)
        {
            jsonArray.Add(indexedContentType);
        }

        context.Data["IndexedContentTypes"] = jsonArray;

        return Task.CompletedTask;
    }

    public override async Task MappingAsync(AzureAISearchMappingContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var metadata = context.Settings.As<ContentIndexMetadata>();

        foreach (var contentType in metadata.IndexedContentTypes ?? [])
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            var index = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(index, contentItem, [contentType], new AzureAISearchContentIndexSettings());
            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            await AddIndexMappingAsync(context.Settings.IndexMappings, IndexingConstants.ContentItemIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), context.Settings);
            await AddIndexMappingAsync(context.Settings.IndexMappings, IndexingConstants.ContentItemVersionIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemVersionIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), context.Settings);

            foreach (var entry in index.Entries)
            {
                if (!AzureAISearchIndexNamingHelper.TryGetSafeFieldName(entry.Name, out var safeFieldName))
                {
                    continue;
                }

                await AddIndexMappingAsync(context.Settings.IndexMappings, safeFieldName, entry, context.Settings);
            }
        }
    }

    private async Task AddIndexMappingAsync(IList<AzureAISearchIndexMap> indexMappings, string safeFieldName, DocumentIndexEntry entry, AzureAISearchIndexSettings settings)
    {
        var indexMap = new AzureAISearchIndexMap(safeFieldName, entry.Type, entry.Options)
        {
            IndexingKey = entry.Name,
        };

        var context = new SearchIndexDefinition(indexMap, entry, settings);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappingAsync(ctx), context, _logger);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappedAsync(ctx), context, _logger);

        indexMappings.Add(indexMap);
    }
}
