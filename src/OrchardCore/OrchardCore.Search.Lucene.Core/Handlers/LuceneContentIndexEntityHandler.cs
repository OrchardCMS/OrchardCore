using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Core.Handlers;

public sealed class LuceneContentIndexEntityHandler : IndexEntityHandlerBase
{
    private readonly IContentManager _contentManager;

    public LuceneContentIndexEntityHandler(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
       => PopulateAsync(context.Model, context.Data);

    public override Task CreatingAsync(CreatingContext<IndexEntity> context)
        => SetMappingsAsync(context.Model);

    public override Task UpdatingAsync(UpdatingContext<IndexEntity> context)
        => SetMappingsAsync(context.Model);

    private async Task SetMappingsAsync(IndexEntity index)
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

    private async Task PopulateAsync(IndexEntity index, JsonNode data)
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

        index.Put(LuceneMetadata);

        var contentMetadata = index.As<LuceneContentIndexMetadata>();

        var storeSourceData = data[nameof(contentMetadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            contentMetadata.StoreSourceData = storeSourceData.Value;
        }

        index.Put(contentMetadata);
    }

    private static bool CanHandle(IndexEntity index)
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
        };

        if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
        {
            return fields;
        }

        foreach (var contentType in metadata.IndexedContentTypes)
        {
            var contentItem = await _contentManager.NewAsync(contentType);

            var document = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(document, contentItem, [contentType], new LuceneContentIndexSettings());

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
