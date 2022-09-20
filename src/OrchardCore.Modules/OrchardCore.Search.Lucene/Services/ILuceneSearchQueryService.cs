using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.Services
{
    public interface ILuceneSearchQueryService
    {
        Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start = 0, int end = 20);
    }
}
