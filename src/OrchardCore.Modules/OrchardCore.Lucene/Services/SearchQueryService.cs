using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Search;
using OrchardCore.ContentManagement;
using OrchardCore.Lucene;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene.Services
{
    public class SearchQueryService : ISearchQueryService
    {
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly IContentManager _contentManager;

        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        public SearchQueryService(
            IContentManager contentManager,
            LuceneIndexManager luceneIndexProvider)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _contentManager = contentManager;

        }
        public async Task<SearchQueryResult> ExecuteQueryAsync(Query query, string indexName, Pager pager)
        {
            var contentItemIds = new List<string>();

            await _luceneIndexProvider.SearchAsync(indexName, searcher =>
            {
                // Fetch one more result than PageSize to generate "More" links
                var collector = TopScoreDocCollector.Create(pager.PageSize + 1, true);

                searcher.Search(query, collector);
                var hits = collector.GetTopDocs(pager.GetStartIndex(), pager.PageSize + 1);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(d.GetField("ContentItemId").GetStringValue());
                }

                return Task.CompletedTask;
            });

            var contentItems = new List<ContentItem>();
            foreach (var contentItemId in contentItemIds.Take(pager.PageSize))
            {
                var contentItem = await _contentManager.GetAsync(contentItemId);
                if (contentItem != null)
                {
                    contentItems.Add(contentItem);
                }
            }
            return new SearchQueryResult
            {
                ContentItems = contentItems,
                ContentItemIds = contentItemIds
            };


        }
    }
}
