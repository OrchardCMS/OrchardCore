
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexManager
{
    Task<bool> CreateAsync(IndexEntity index);

    Task<bool> RebuildAsync(IndexEntity index);

    Task<bool> DeleteAsync(IndexEntity index);

    Task<bool> ExistsAsync(string indexFullName);
}
