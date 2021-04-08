using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace OrchardCore.Search.Elastic
{
    public interface ISearchQueryService
    {
        Task<IList<string>> ExecuteQueryAsync(string query, string indexName, int start = 0, int end = 20);
    }
}
