using Lucene.Net.Search;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Lucene.Core;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchQueryService : ILuceneSearchQueryService
{
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly ILuceneIndexStore _store;

    public LuceneSearchQueryService(
        IIndexEntityStore indexEntityStore,
        ILuceneIndexStore store)
    {
        _indexEntityStore = indexEntityStore;
        _store = store;
    }

    public async Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start, int end)
    {
        var documentIds = new List<string>();

        if (string.IsNullOrWhiteSpace(indexName))
        {
            return documentIds;
        }

        var index = await _indexEntityStore.FindByIndexNameAndProviderAsync(indexName, LuceneConstants.ProviderName);

        if (index is null)
        {
            return documentIds;
        }

        await _store.SearchAsync(index, searcher =>
        {
            if (end > 0)
            {
                var metadata = index.As<LuceneIndexMetadata>();
                var idSets = new HashSet<string>() { metadata.IndexMappings.KeyFieldName };
                var collector = TopScoreDocCollector.Create(end, true);

                searcher.Search(query, collector);
                var hits = collector.GetTopDocs(start, end);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, idSets);
                    documentIds.Add(d.GetField(metadata.IndexMappings.KeyFieldName).GetStringValue());
                }
            }

            return Task.CompletedTask;
        });

        return documentIds;
    }
}
