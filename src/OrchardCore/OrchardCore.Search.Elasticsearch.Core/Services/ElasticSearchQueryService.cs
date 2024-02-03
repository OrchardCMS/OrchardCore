using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    public class ElasticSearchQueryService : IElasticSearchQueryService
    {
        private readonly ElasticIndexManager _elasticIndexManager;

        public ElasticSearchQueryService(ElasticIndexManager elasticIndexManager)
        {
            _elasticIndexManager = elasticIndexManager;
        }

        public async Task<IList<string>> ExecuteQueryAsync(string indexName, QueryContainer query, List<ISort> sort, int from, int size)
        {
            var contentItemIds = new List<string>();

            var results = await _elasticIndexManager.SearchAsync(indexName, query, sort, from, size);

            foreach (var item in results.TopDocs)
            {
                contentItemIds.Add(item.GetValueOrDefault("ContentItemId").ToString());
            }

            return contentItemIds;
        }
    }
}
