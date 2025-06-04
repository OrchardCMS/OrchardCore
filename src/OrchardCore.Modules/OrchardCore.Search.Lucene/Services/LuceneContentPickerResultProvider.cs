using Lucene.Net.Index;
using Lucene.Net.Search;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Lucene.Core;
using OrchardCore.Search.Lucene.Settings;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneContentPickerResultProvider : IContentPickerResultProvider
{
    private readonly IIndexEntityStore _indexStore;
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly ILuceneIndexStore _luceneIndexStore;

    public LuceneContentPickerResultProvider(
        IIndexEntityStore indexStore,
        LuceneIndexManager luceneIndexManager,
        ILuceneIndexStore luceneIndexStore)
    {
        _indexStore = indexStore;
        _luceneIndexManager = luceneIndexManager;
        _luceneIndexStore = luceneIndexStore;
    }

    public string Name => LuceneConstants.ProviderName;

    public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
    {
        var indexName = "Search";

        var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldLuceneEditorSettings>();

        if (!string.IsNullOrWhiteSpace(fieldSettings?.Index))
        {
            indexName = fieldSettings.Index;
        }

        if (string.IsNullOrWhiteSpace(fieldSettings?.Index))
        {
            return [];
        }

        var index = await _indexStore.FindByIndexNameAndProviderAsync(fieldSettings.Index, LuceneConstants.ProviderName);

        if (index is null || index.Type != IndexingConstants.ContentsIndexSource || !await _luceneIndexManager.ExistsAsync(index.IndexFullName))
        {
            return [];
        }

        var results = new List<ContentPickerResult>();

        await _luceneIndexStore.SearchAsync(index, searcher =>
        {
            Query query = null;

            if (string.IsNullOrWhiteSpace(searchContext.Query))
            {
                query = new MatchAllDocsQuery();
            }
            else
            {
                query = new WildcardQuery(new Term("Content.ContentItem.DisplayText.Normalized", searchContext.Query.ToLowerInvariant() + "*"));
            }

            var filter = new FieldCacheTermsFilter("Content.ContentItem.ContentType", searchContext.ContentTypes.ToArray());

            var docs = searcher.Search(query, filter, 50, Sort.RELEVANCE);

            foreach (var hit in docs.ScoreDocs)
            {
                var doc = searcher.Doc(hit.Doc);

                results.Add(new ContentPickerResult
                {
                    ContentItemId = doc.GetField("ContentItemId").GetStringValue(),
                    DisplayText = doc.GetField("Content.ContentItem.DisplayText.keyword").GetStringValue(),
                    HasPublished = doc.GetField("Content.ContentItem.Published").GetStringValue().Equals("true", StringComparison.OrdinalIgnoreCase),
                });
            }

            return Task.CompletedTask;
        });

        return results.OrderBy(x => x.DisplayText);
    }
}
