
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexManager
{
    Task<bool> CreateAsync(IndexProfile indexProfile);

    Task<bool> RebuildAsync(IndexProfile indexProfile);

    Task<bool> DeleteAsync(IndexProfile indexProfile);

    Task<bool> ExistsAsync(string indexFullName);
}
