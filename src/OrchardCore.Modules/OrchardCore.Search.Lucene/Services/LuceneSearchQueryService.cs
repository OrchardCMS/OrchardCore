using Lucene.Net.Search;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchQueryService : ILuceneSearchQueryService
{
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly LuceneIndexManager _luceneIndexManager;

    private static readonly HashSet<string> _idSet = new(["ContentItemId"]);

    public LuceneSearchQueryService(
        IIndexEntityStore indexEntityStore,
        LuceneIndexManager luceneIndexManager)
    {
        _indexEntityStore = indexEntityStore;
        _luceneIndexManager = luceneIndexManager;
    }

    public async Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start, int end)
    {
        var contentItemIds = new List<string>();

        if (string.IsNullOrWhiteSpace(indexName))
        {
            return contentItemIds;
        }

        var index = await _indexEntityStore.FindByNameAndProviderAsync(LuceneConstants.ProviderName, indexName);

        if (index is null)
        {
            return contentItemIds;
        }

        await _luceneIndexManager.SearchAsync(index, searcher =>
        {
            if (end > 0)
            {
                var collector = TopScoreDocCollector.Create(end, true);

                searcher.Search(query, collector);
                var hits = collector.GetTopDocs(start, end);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, _idSet);
                    contentItemIds.Add(d.GetField("ContentItemId").GetStringValue());
                }
            }

            return Task.CompletedTask;
        });

        return contentItemIds;
    }
}
