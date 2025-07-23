using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexProfileStore
{
    /// <summary>
    /// Asynchronously retrieves a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is the model if found, or <c>null</c> if no matching model exists in the store.
    /// </returns>
    ValueTask<IndexProfile> FindByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves a model by its index name and provider name.
    /// </summary>
    /// <param name="indexName">The name of the index. Must not be <c>null</c> or empty.</param>
    /// <param name="providerName">The name of the provider. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is the <see cref="IndexProfile"/> if found, or <c>null</c> if no matching model exists in the store.
    /// </returns>
    ValueTask<IndexProfile> FindByIndexNameAndProviderAsync(string indexName, string providerName);

    /// <summary>
    /// Asynchronously retrieves all models from the store.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all models in the store.
    /// </returns>
    ValueTask<IEnumerable<IndexProfile>> GetAllAsync();

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
    ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext;

    /// <summary>
    /// Asynchronously deletes the specified model from the store.
    /// </summary>
    /// <param name="indexProfile">The model to delete. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// The result is <c>true</c> if the deletion was successful, <c>false</c> if the model does not exist or could not be deleted.
    /// </returns>
    ValueTask<bool> DeleteAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously creates the specified model in the store.
    /// </summary>
    /// <param name="indexProfile">The model to create. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask CreateAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously updates the specified model in the store.
    /// </summary>
    /// <param name="indexProfile">The model to update. Must not be <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask UpdateAsync(IndexProfile indexProfile);

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
    ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName);

    /// <summary>
    /// Asynchronously finds data sources by the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<IndexProfile>> GetByTypeAsync(string type);

    /// <summary>
    /// Asynchronously finds data sources by the given provider-name and type.
    /// </summary>
    /// <param name="providerName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type);

    /// <summary>
    /// Retrieves an existing <see cref="IndexProfile"/> by its unique display name.
    /// </summary>
    /// <returns>
    /// The <see cref="IndexProfile"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Used to ensure that no other index entity exists with the same display name.
    /// </remarks>
    ValueTask<IndexProfile> FindByNameAsync(string name);
}
