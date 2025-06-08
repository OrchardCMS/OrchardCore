using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.Abstractions;

public interface ISearchService
{
    Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int size);
}
