using System.Threading.Tasks;
using Lucene.Net.Search;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene.Services
{
    public interface ISearchQueryService
    {
        Task<SearchQueryResult> ExecuteQueryAsync(Query query, string indexName, Pager pager);
    }
}