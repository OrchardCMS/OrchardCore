using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;
using OrchardCore.ContentManagement;

namespace OrchardCore.Lucene.Services
{
    public class SearchQueryService : ISearchQueryService
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly IContentManager _contentManager;

        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        public SearchQueryService(
            IContentManager contentManager,
            LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
            _contentManager = contentManager;

        }
        public async Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start, int end)
        {
            var contentItemIds = new List<string>();

            await _luceneIndexManager.SearchAsync(indexName, searcher =>
            {                
                var collector = TopScoreDocCollector.Create(end, true);

                searcher.Search(query, collector);
                var hits = collector.GetTopDocs(start, end);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(d.GetField("ContentItemId").GetStringValue());
                }

                return Task.CompletedTask;
            });

            return contentItemIds;

        }
    }
}