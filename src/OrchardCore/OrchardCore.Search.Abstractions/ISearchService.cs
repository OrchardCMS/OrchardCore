using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Abstractions;

public interface ISearchService
{
    bool CanHandle(SearchProvider provider);

    Task<Permission> GetPermissionAsync(string indexName);

    Task<bool> ExistsAsync(string indexName);

    Task<string> DefaultIndexAsync();

    Task<SearchResult> GetAsync(string indexName, string term, string[] defaultSearchFields, int start, int size);

    Task<string[]> GetSearchFieldsAsync(string indexName);
}
