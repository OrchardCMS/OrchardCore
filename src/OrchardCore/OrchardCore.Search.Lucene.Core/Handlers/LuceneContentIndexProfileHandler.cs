using System.Text.Json.Nodes;
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
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Core.Handlers;

public sealed class LuceneContentIndexProfileHandler : IndexProfileHandlerBase
{
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IDocumentIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;

    public LuceneContentIndexProfileHandler(
        IContentManager contentManager,
        IEnumerable<IDocumentIndexHandler> contentItemIndexHandlers,
        ILogger<LuceneContentIndexProfileHandler> logger)
    {
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _logger = logger;
    }

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
       => PopulateAsync(context.Model, context.Data);

    public override Task CreatingAsync(CreatingContext<IndexProfile> context)
        => SetMappingsAsync(context.Model);

    public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        => SetMappingsAsync(context.Model);

    private async Task SetMappingsAsync(IndexProfile index)
    {
        if (!CanHandle(index))
        {
            return;
        }

        var LuceneMetadata = index.As<LuceneIndexMetadata>();

        var map = new LuceneIndexMap()
        {
            KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
        };

        var metadata = index.As<ContentIndexMetadata>();

        map.Fields = (await PopulateTypeMappingAsync(metadata)).ToArray();

        LuceneMetadata.IndexMappings = map;

        index.Put(LuceneMetadata);
    }

    private async Task PopulateAsync(IndexProfile index, JsonNode data)
    {
        if (!CanHandle(index))
        {
            return;
        }

        var LuceneMetadata = index.As<LuceneIndexMetadata>();

        var map = new LuceneIndexMap()
        {
            KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
        };

        var metadata = index.As<ContentIndexMetadata>();

        map.Fields = (await PopulateTypeMappingAsync(metadata)).ToArray();

        LuceneMetadata.IndexMappings = map;

        var analyzerName = data[nameof(LuceneMetadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            LuceneMetadata.AnalyzerName = analyzerName;
        }

        var storeSourceData = data[nameof(LuceneMetadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            LuceneMetadata.StoreSourceData = storeSourceData.Value;
        }

        index.Put(LuceneMetadata);

        var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

        if (queryMetadata.DefaultSearchFields is null || queryMetadata.DefaultSearchFields.Length == 0)
        {
            queryMetadata.DefaultSearchFields = [ContentIndexingConstants.FullTextKey];
        }
    }

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(LuceneConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<HashSet<string>> PopulateTypeMappingAsync(ContentIndexMetadata metadata)
    {
        var fields = new HashSet<string>()
        {
            ContentIndexingConstants.ContentItemIdKey,
            ContentIndexingConstants.ContentItemVersionIdKey,
            ContentIndexingConstants.FullTextKey,
        };

        if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
        {
            return fields;
        }

        foreach (var contentType in metadata.IndexedContentTypes)
        {
            var contentItem = await _contentManager.NewAsync(contentType);

            var document = new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildDocumentIndexContext(document, contentItem, [contentType], new LuceneContentIndexSettings());

            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var entry in document.Entries)
            {
                if (entry.Name == ContentIndexingConstants.ContentItemIdKey || entry.Name == ContentIndexingConstants.ContentItemVersionIdKey)
                {
                    continue;
                }

                fields.Add(entry.Name);
            }
        }

        return fields;
    }
}
