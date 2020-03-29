using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace OrchardCore.Lucene.Services
{
    public interface ISearchQueryService
    {
        Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start = 0, int end = 20);
    }
}
