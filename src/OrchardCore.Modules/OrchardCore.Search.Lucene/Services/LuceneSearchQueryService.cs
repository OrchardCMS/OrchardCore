using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.Services
{
    public class LuceneSearchQueryService : ILuceneSearchQueryService
    {
        private readonly LuceneIndexManager _luceneIndexManager;

        private static readonly HashSet<string> _idSet = new(new string[] { "ContentItemId" });

        public LuceneSearchQueryService(LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
        }

        public async Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start, int end)
        {
            var contentItemIds = new List<string>();

            await _luceneIndexManager.SearchAsync(indexName, searcher =>
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
}
