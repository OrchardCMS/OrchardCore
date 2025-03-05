using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Services;

public class ContentAzureAISearchEvents : IAzureAISearchEvents
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    public ContentAzureAISearchEvents(
        IContentManager contentManager,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IEnumerable<IAzureAISearchFieldIndexEvents> fieldIndexEvents,
        ILogger<ContentAzureAISearchEvents> logger)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _logger = logger;
    }

    public async Task MappingAsync(AzureAISearchMappingContext context)
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

            await AddIndexMappingAsync(context.Mappings, IndexingConstants.ContentItemIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), context.Settings);
            await AddIndexMappingAsync(context.Mappings, IndexingConstants.ContentItemVersionIdKey, new DocumentIndexEntry(IndexingConstants.ContentItemVersionIdKey, contentItem.ContentItemId, Types.Text, DocumentIndexOptions.Keyword), context.Settings);

            foreach (var entry in index.Entries)
            {
                if (!AzureAISearchIndexNamingHelper.TryGetSafeFieldName(entry.Name, out var safeFieldName))
                {
                    continue;
                }

                await AddIndexMappingAsync(context.Mappings, safeFieldName, entry, context.Settings);
            }
        }
    }

    private async Task AddIndexMappingAsync(List<AzureAISearchIndexMap> indexMappings, string safeFieldName, DocumentIndexEntry entry, AzureAISearchIndexSettings settings)
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
