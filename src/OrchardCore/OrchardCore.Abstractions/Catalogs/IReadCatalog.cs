using OrchardCore.Catalogs.Models;

namespace OrchardCore.Catalogs;

public interface IReadCatalog<T>
{
    /// <summary>
    /// Asynchronously retrieves a entry by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entry. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is the entry if found, or <c>null</c> if no matching entry exists in the catalog.
    /// </returns>
    ValueTask<T> FindByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves all entry from the catalog.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all entry in the catalog.
    /// </returns>
    ValueTask<IReadOnlyCollection<T>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves all entry from the catalog.
    /// </summary>
    /// <returns>
    /// <param name="ids">The ids to retrieve.</param>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all entry in the catalog.
    /// </returns>
    ValueTask<IReadOnlyCollection<T>> GetAsync(IEnumerable<string> ids);

    /// <summary>
    /// Asynchronously retrieves a paginated list of entry based on the specified pagination and filtering parameters.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query context used for filtering, sorting, and other query options.</typeparam>
    /// <param name="page">The page number to retrieve (1-based index).</param>
    /// <param name="pageSize">The number of entry to retrieve per page.</param>
    /// <param name="context">The query context containing filtering, sorting, and search parameters. Can be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is a <see cref="PageResult{T}"/> containing the entry for the requested page, along with pagination metadata.
    /// </returns>
    ValueTask<PageResult<T>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext;
}
