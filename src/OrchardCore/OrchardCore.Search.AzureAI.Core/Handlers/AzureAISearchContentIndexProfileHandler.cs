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
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchContentIndexProfileHandler : IndexProfileHandlerBase
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    private readonly IStringLocalizer S;

    public AzureAISearchContentIndexProfileHandler(
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IEnumerable<IAzureAISearchFieldIndexEvents> fieldIndexEvents,
        ILogger<AzureAISearchContentIndexProfileHandler> logger,
        IStringLocalizer<AzureAISearchContentIndexProfileHandler> stringLocalizer)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _logger = logger;
        S = stringLocalizer;
    }

    public override Task CreatingAsync(CreatingContext<IndexProfile> context)
        => SetMappingAsync(context.Model);

    public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        => SetMappingAsync(context.Model);

    public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
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

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
       => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexProfile index, JsonNode data)
    {
        if (!CanHandle(index))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<AzureAISearchIndexMetadata>();

        // For backward compatibility, we look for 'AnalyzerName' and 'QueryAnalyzerName' in the data.
        var analyzerName = data[nameof(metadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            metadata.AnalyzerName = analyzerName;
        }

        var indexMappings = data[nameof(metadata.IndexMappings)]?.AsArray();

        if (indexMappings is not null)
        {
            foreach (var indexMapping in indexMappings)
            {
                metadata.IndexMappings.Add(indexMapping.GetValue<AzureAISearchIndexMap>());
            }
        }

        index.Put(metadata);

        var queryMetadata = index.As<AzureAISearchDefaultQueryMetadata>();

        var queryAnalyzerName = data[nameof(queryMetadata.QueryAnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(queryAnalyzerName))
        {
            queryMetadata.QueryAnalyzerName = queryAnalyzerName;
        }

        var defaultFields = data[nameof(queryMetadata.DefaultSearchFields)]?.AsArray();

        if (defaultFields is not null && defaultFields.Count > 0)
        {
            var fields = new List<string>();

            foreach (var field in defaultFields)
            {
                fields.Add(field.GetValue<string>());
            }

            queryMetadata.DefaultSearchFields = fields.ToArray();
        }

        index.Put(queryMetadata);


        return Task.CompletedTask;
    }

    public override Task ExportingAsync(IndexProfileExportingContext context)
    {
        if (!CanHandle(context.IndexProfile))
        {
            return Task.CompletedTask;
        }

        var metadata = context.IndexProfile.As<AzureAISearchIndexMetadata>();

        context.Data["AnalyzerName"] = metadata.AnalyzerName;
        context.Data["QueryAnalyzerName"] = context.IndexProfile.As<AzureAISearchDefaultQueryMetadata>().QueryAnalyzerName;
        context.Data["IndexMappings"] = JArray.FromObject(metadata.IndexMappings);

        return Task.CompletedTask;
    }

    private async Task SetMappingAsync(IndexProfile index)
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

            var document = new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
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

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(AzureAISearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddIndexMappingAsync(IList<AzureAISearchIndexMap> indexMappings, string safeFieldName, DocumentIndexEntry entry, IndexProfile index)
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
