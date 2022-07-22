using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elastic
{
    public class SearchQueryService : ISearchQueryService
    {
        private readonly ElasticIndexManager _elasticIndexManager;

        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        public SearchQueryService(ElasticIndexManager elasticIndexManager)
        {
            _elasticIndexManager = elasticIndexManager;
        }

        public async Task<IList<string>> ExecuteQueryAsync(string query, string indexName, int start, int end)
        {
            var contentItemIds = new List<string>();

            var results = await _elasticIndexManager.SearchAsync(indexName, query);

            foreach (var item in results.TopDocs)
            {
                contentItemIds.Add(item.GetValueOrDefault("ContentItemId").ToString());
            }

            //Here return the contentItemIds
            return contentItemIds;
        }

        public async Task<IList<string>> ExecuteQueryAsync(QueryContainer query, string indexName, int from, int size)
        {
            var contentItemIds = new List<string>();

            var results = await _elasticIndexManager.SearchAsync(indexName, query, from, size);

            foreach (var item in results.TopDocs)
            {
               contentItemIds.Add(item.GetValueOrDefault("ContentItemId").ToString());
            }

            //Here return the contentItemIds
            return contentItemIds;
        }
    }
}
