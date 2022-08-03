using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public class SearchQueryService : ISearchQueryService
    {
        private readonly ElasticsearchIndexManager _elasticIndexManager;

        public SearchQueryService(ElasticsearchIndexManager elasticIndexManager)
        {
            _elasticIndexManager = elasticIndexManager;
        }

        public async Task<IList<string>> ExecuteQueryAsync(string query, string indexName, int from, int size)
        {
            var contentItemIds = new List<string>();

            var results = await _elasticIndexManager.SearchAsync(indexName, query);

            foreach (var item in results.TopDocs)
            {
                contentItemIds.Add(item.GetValueOrDefault("ContentItemId").ToString());
            }

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

            return contentItemIds;
        }
    }
}
