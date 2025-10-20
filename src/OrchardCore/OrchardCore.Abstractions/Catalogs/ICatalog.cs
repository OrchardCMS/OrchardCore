namespace OrchardCore.Catalogs;

public interface ICatalog<T> : IReadCatalog<T>
{
    /// <summary>
    /// Asynchronously deletes the specified entry from the catalog.
    /// </summary>
    /// <param name="entry">The entry to delete. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is <c>true</c> if the deletion was successful, <c>false</c> if the entry does not exist or could not be deleted.
    /// </returns>
    ValueTask<bool> DeleteAsync(T entry);

    /// <summary>
    /// Asynchronously creates the specified entry in the catalog.
    /// </summary>
    /// <param name="entry">The entry to create. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask CreateAsync(T entry);

    /// <summary>
    /// Asynchronously updates the specified entry in the catalog.
    /// </summary>
    /// <param name="entry">The entry to update. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask UpdateAsync(T entry);

    /// <summary>
    /// Asynchronously saves all pending changes in the catalog.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask SaveChangesAsync();
}
