using System.Threading.Tasks;

namespace OrchardCore.Search.Abstractions;

public interface ISearchService
{
    string Name { get; }

    Task<SearchResult> SearchAsync(string indexName, string term, int start, int size);
}
