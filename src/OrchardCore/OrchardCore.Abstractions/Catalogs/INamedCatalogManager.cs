namespace OrchardCore.Catalogs;

public interface INamedCatalogManager<T> : ICatalogManager<T>
    where T : INameAwareModel
{
    /// <summary>
    /// Asynchronously retrieves a model by its name.
    /// </summary>
    /// <param name="name">The name of the model. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> representing the asynchronous operation.
    /// The result is the model matching the specified name, or <c>null</c> if no model is found.
    /// </returns>
    ValueTask<T> FindByNameAsync(string name);
}
