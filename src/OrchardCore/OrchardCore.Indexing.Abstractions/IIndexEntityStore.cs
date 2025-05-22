using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexEntityStore
{
    /// <summary>
    /// Asynchronously retrieves a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is the model if found, or <c>null</c> if no matching model exists in the store.
    /// </returns>
    ValueTask<IndexEntity> FindByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves all models from the store.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all models in the store.
    /// </returns>
    ValueTask<IEnumerable<IndexEntity>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves a paginated list of models based on the specified pagination and filtering parameters.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query context used for filtering, sorting, and other query options.</typeparam>
    /// <param name="page">The page number to retrieve (1-based index).</param>
    /// <param name="pageSize">The number of models to retrieve per page.</param>
    /// <param name="context">The query context containing filtering, sorting, and search parameters. Can be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is a <see cref="PageResult{T}"/> containing the models for the requested page, along with pagination metadata.
    /// </returns>
    ValueTask<PageResult<IndexEntity>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext;

    /// <summary>
    /// Asynchronously deletes the specified model from the store.
    /// </summary>
    /// <param name="model">The model to delete. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is <c>true</c> if the deletion was successful, <c>false</c> if the model does not exist or could not be deleted.
    /// </returns>
    ValueTask<bool> DeleteAsync(IndexEntity model);

    /// <summary>
    /// Asynchronously creates the specified model in the store.
    /// </summary>
    /// <param name="model">The model to create. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask CreateAsync(IndexEntity model);

    /// <summary>
    /// Asynchronously updates the specified model in the store.
    /// </summary>
    /// <param name="model">The model to update. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask UpdateAsync(IndexEntity model);

    /// <summary>
    /// Asynchronously saves all pending changes in the store.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask SaveChangesAsync();

    /// <summary>
    /// Asynchronously finds data sources by the given provider-name.
    /// </summary>
    /// <param name="providerName"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<IndexEntity>> GetAsync(string providerName);

    /// <summary>
    /// Asynchronously finds data sources by the given provider-name and type.
    /// </summary>
    /// <param name="providerName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<IndexEntity>> GetAsync(string providerName, string type);
}
