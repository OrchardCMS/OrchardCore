using System.Threading.Tasks;

namespace OrchardCore.Search.Abstractions;

public interface ISearchService
{
    bool CanHandle(SearchProvider provider);

    Task<SearchResult> GetAsync(string indexName, string term, int start, int size);
}
