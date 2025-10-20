using OrchardCore.Catalogs.Models;

namespace OrchardCore.Catalogs;

public interface IReadCatalogManager<T>
{
    /// <summary>
    /// Asynchronously retrieves a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is the model corresponding to the specified ID, or <c>null</c> if not found.
    /// </returns>
    ValueTask<T> FindByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves a list of all models.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all models.
    /// </returns>
    ValueTask<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves a paginated list of models.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query context used for filtering, sorting, and other query options.</typeparam>
    /// <param name="page">The page number of the results to retrieve.</param>
    /// <param name="pageSize">The number of results per page.</param>
    /// <param name="context">The query context containing filtering, sorting, and other parameters.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is a <see cref="PageResult{T}"/> containing the paginated query results.
    /// </returns>
    ValueTask<PageResult<T>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext;
}
