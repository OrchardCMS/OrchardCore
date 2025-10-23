namespace OrchardCore.Catalogs;

public interface INamedSourceCatalogManager<T> : INamedCatalogManager<T>, ISourceCatalogManager<T>
    where T : INameAwareModel, ISourceAwareModel
{
    /// <summary>
    /// Asynchronously retrieves a model by its name and source.
    /// </summary>
    /// <param name="name">The unique name of the model. Must not be <c>null</c> or empty.</param>
    /// <param name="source">The unique identifier of the source provider. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> representing the asynchronous operation.
    /// The result is the model matching the specified name and source, or <c>null</c> if no such model exists.
    /// </returns>
    ValueTask<T> GetAsync(string name, string source);
}
