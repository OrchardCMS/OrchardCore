using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene.Services
{
    public interface ISearchQueryService
    {
        Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, Pager pager);
    }
}