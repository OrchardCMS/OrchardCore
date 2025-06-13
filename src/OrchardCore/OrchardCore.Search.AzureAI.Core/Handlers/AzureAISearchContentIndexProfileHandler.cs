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
    private readonly IEnumerable<IDocumentIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public AzureAISearchContentIndexProfileHandler(
        IContentManager contentManager,
        IEnumerable<IDocumentIndexHandler> contentItemIndexHandlers,
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
            var buildIndexContext = new BuildDocumentIndexContext(document, contentItem, [contentType], new AzureAISearchContentIndexSettings());
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
