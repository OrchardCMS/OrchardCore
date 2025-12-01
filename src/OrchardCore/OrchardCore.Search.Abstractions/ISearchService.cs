using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.Abstractions;

public interface ISearchService
{
    string Name { get; }

    Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int size);
}
