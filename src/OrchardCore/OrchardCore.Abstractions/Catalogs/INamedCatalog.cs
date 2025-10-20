namespace OrchardCore.Catalogs;

public interface INamedCatalog<T> : ICatalog<T>
    where T : INameAwareModel
{
    /// <summary>
    /// Asynchronously retrieves a entry by its unique name.
    /// </summary>
    /// <param name="name">The unique name of the entry. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> representing the asynchronous operation.
    /// The result is the entry if found, or <c>null</c> if no entry with the specified name exists in the catalog.
    /// </returns>
    ValueTask<T> FindByNameAsync(string name);
}
