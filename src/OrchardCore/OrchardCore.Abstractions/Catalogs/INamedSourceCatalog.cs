namespace OrchardCore.Catalogs;

public interface INamedSourceCatalog<T> : INamedCatalog<T>, ISourceCatalog<T>
    where T : INameAwareModel, ISourceAwareModel
{
    /// <summary>
    /// Asynchronously retrieves a entry by its unique name and source.
    /// </summary>
    /// <param name="name">The unique name of the entry. Must not be <c>null</c> or empty.</param>
    /// <param name="source">The source of the entry. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> representing the asynchronous operation.
    /// The result is the entry if found, or <c>null</c> if no entry with the specified name and source exists in the catalog.
    /// </returns>
    ValueTask<T> GetAsync(string name, string source);
}
