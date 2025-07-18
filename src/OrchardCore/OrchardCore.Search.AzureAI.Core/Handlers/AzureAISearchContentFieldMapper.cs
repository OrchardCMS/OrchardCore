using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchContentFieldMapper
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IDocumentIndexHandler> _contentItemIndexHandlers;
    private readonly IEnumerable<IAzureAISearchFieldIndexEvents> _fieldIndexEvents;
    private readonly ILogger _logger;

    public AzureAISearchContentFieldMapper(
        IContentManager contentManager,
        IEnumerable<IDocumentIndexHandler> contentItemIndexHandlers,
        IEnumerable<IAzureAISearchFieldIndexEvents> fieldIndexEvents,
        ILogger<AzureAISearchContentFieldMapper> logger)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _fieldIndexEvents = fieldIndexEvents;
        _logger = logger;
    }

    public async Task MapAsync(IList<AzureAISearchIndexMap> fields, IndexProfile indexProfile, IEnumerable<string> contentTypes, bool rootFields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(indexProfile);

        await AddIndexMappingAsync(
            fields,
            ContentIndexingConstants.ContentItemIdKey,
            new DocumentIndexEntry(ContentIndexingConstants.ContentItemIdKey, value: null, Types.Text, DocumentIndexOptions.Keyword),
            indexProfile,
            rootFields);

        await AddIndexMappingAsync(
            fields,
            ContentIndexingConstants.ContentItemVersionIdKey,
            new DocumentIndexEntry(ContentIndexingConstants.ContentItemVersionIdKey, value: null, Types.Text, DocumentIndexOptions.Keyword),
            indexProfile,
            rootFields);

        if (contentTypes is null || !contentTypes.Any())
        {
            return;
        }

        foreach (var contentType in contentTypes)
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

                await AddIndexMappingAsync(fields, safeFieldName, entry, indexProfile, rootFields);
            }
        }
    }

    private async Task AddIndexMappingAsync(IList<AzureAISearchIndexMap> indexMappings, string safeFieldName, DocumentIndexEntry entry, IndexProfile index, bool isRootField)
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

        var context = new SearchIndexDefinition(indexMap, entry, index)
        {
            IsRoolField = isRootField,
        };

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappingAsync(ctx), context, _logger);

        await _fieldIndexEvents.InvokeAsync((handler, ctx) => handler.MappedAsync(ctx), context, _logger);
    }
}
