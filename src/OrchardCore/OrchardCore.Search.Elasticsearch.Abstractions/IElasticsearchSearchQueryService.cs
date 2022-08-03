using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticsearchSearchQueryService
    {
        Task<IList<string>> ExecuteQueryAsync(string query, string indexName, int start = 0, int end = 20);
        Task<IList<string>> ExecuteQueryAsync(QueryContainer query, string indexName, int start = 0, int end = 20);
    }
}
