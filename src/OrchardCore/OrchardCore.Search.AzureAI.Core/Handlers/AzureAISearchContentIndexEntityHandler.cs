using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchContentIndexEntityHandler : IndexEntityHandlerBase
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    private readonly IStringLocalizer S;

    public AzureAISearchContentIndexEntityHandler(
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IEnumerable<IAzureAISearchFieldIndexEvents> fieldIndexEvents,
        ILogger<AzureAISearchContentIndexEntityHandler> logger,
        IStringLocalizer<AzureAISearchContentIndexEntityHandler> stringLocalizer)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _logger = logger;
        S = stringLocalizer;
    }

    public override Task CreatingAsync(CreatingContext<IndexEntity> context)
        => SetMappingAsync(context.Model);

    public override Task UpdatingAsync(UpdatingContext<IndexEntity> context)
        => SetMappingAsync(context.Model);

    public override Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (string.Equals(AzureAISearchConstants.ProviderName, context.Model.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            // When the provider is AzureAI, "regardless of the type" we need to validate the index mappings.
            var metadata = context.Model.As<AzureAISearchIndexMetadata>();

            if (metadata.IndexMappings is null || metadata.IndexMappings.Count == 0)
            {
                context.Result.Fail(new ValidationResult(S["At least one mapping field is required."]));
            }
        }

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
       => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        if (index.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<AzureAISearchIndexMetadata>();

        // For backward compatibility, we look for 'AnalyzerName' and 'QueryAnalyzerName' in the data.
        var analyzerName = data[nameof(AzureAISearchIndexMetadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            metadata.AnalyzerName = analyzerName;
        }

        var queryAnalyzerName = data[nameof(AzureAISearchIndexMetadata.QueryAnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(queryAnalyzerName))
        {
            metadata.QueryAnalyzerName = queryAnalyzerName;
        }

        var indexMappings = data[nameof(AzureAISearchIndexMetadata.IndexMappings)]?.AsArray();

        if (indexMappings is not null)
        {
            foreach (var indexMapping in indexMappings)
            {
                metadata.IndexMappings.Add(indexMapping.GetValue<AzureAISearchIndexMap>());
            }
        }

        index.Put(metadata);

        return Task.CompletedTask;
    }

    public override Task ExportingAsync(IndexEntityExportingContext context)
    {
        if (!CanHandle(context.Index))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Index.As<AzureAISearchIndexMetadata>();

        context.Data["AnalyzerName"] = metadata.AnalyzerName;
        context.Data["QueryAnalyzerName"] = metadata.QueryAnalyzerName;

        var jsonArray = new JsonArray();

        foreach (var IndexMapping in metadata.IndexMappings)
        {
            jsonArray.Add(IndexMapping);
        }

        context.Data["IndexMappings"] = jsonArray;

        return Task.CompletedTask;
    }

    private async Task SetMappingAsync(IndexEntity index)
    {
        if (!CanHandle(index))
        {
            return;
        }

        var metadata = index.As<ContentIndexMetadata>();
        var azureMetadata = index.As<AzureAISearchIndexMetadata>();

        await AddIndexMappingAsync(azureMetadata.IndexMappings, ContentIndexingConstants.ContentItemIdKey, new DocumentIndexEntry(ContentIndexingConstants.ContentItemIdKey, value: null, Types.Text, DocumentIndexOptions.Keyword), index);
        await AddIndexMappingAsync(azureMetadata.IndexMappings, ContentIndexingConstants.ContentItemVersionIdKey, new DocumentIndexEntry(ContentIndexingConstants.ContentItemVersionIdKey, value: null, Types.Text, DocumentIndexOptions.Keyword), index);

        foreach (var contentType in metadata.IndexedContentTypes ?? [])
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            var document = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(document, contentItem, [contentType], new AzureAISearchContentIndexSettings());
            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var entry in document.Entries)
            {
                if (entry.Name == ContentIndexingConstants.ContentItemIdKey || entry.Name == ContentIndexingConstants.ContentItemVersionIdKey)
                {
                    continue;
                }

                if (!AzureAISearchIndexNamingHelper.TryGetSafeFieldName(entry.Name, out var safeFieldName))
                {
                    continue;
                }

                await AddIndexMappingAsync(azureMetadata.IndexMappings, safeFieldName, entry, index);
            }
        }

        index.Put(azureMetadata);
    }

    private static bool CanHandle(IndexEntity index)
    {
        return string.Equals(AzureAISearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddIndexMappingAsync(IList<AzureAISearchIndexMap> indexMappings, string safeFieldName, DocumentIndexEntry entry, IndexEntity index)
    {
        var indexMap = indexMappings.FirstOrDefault(x => x.AzureFieldKey == safeFieldName);

        if (indexMap is null)
        {
            indexMap = new AzureAISearchIndexMap(safeFieldName, entry.Type, entry.Options)
            {
                IndexingKey = entry.Name,
            };

            // Only add the mapping if it doesn't already exist. Otherwise, we update the mapping that already exists.
            indexMappings.Add(indexMap);
        }

        var context = new SearchIndexDefinition(indexMap, entry, index);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappingAsync(ctx), context, _logger);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappedAsync(ctx), context, _logger);
    }
}
