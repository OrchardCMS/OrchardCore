using System.Text.Json.Nodes;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing;

public interface IIndexProfileManager
{
    /// <summary>
    /// Asynchronously retrieves a model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is the model corresponding to the specified ID, or <c>null</c> if not found.
    /// </returns>
    ValueTask<IndexProfile> FindByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves a model by its index name and provider name.
    /// </summary>
    /// <param name="indexName">The name of the index to retrieve.</param>
    /// <param name="providerName">The name of the provider associated with the index.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is the <see cref="IndexProfile"/> corresponding to the specified index and provider, or <c>null</c> if not found.
    /// </returns>
    ValueTask<IndexProfile> FindByNameAndProviderAsync(string indexName, string providerName);

    /// <summary>
    /// Asynchronously retrieves a list of all models.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is an <see cref="IEnumerable{T}"/> containing all models.
    /// </returns>
    ValueTask<IEnumerable<IndexProfile>> GetAllAsync();

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
    ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext;

    /// <summary>
    /// Asynchronously deletes the specified model.
    /// </summary>
    /// <param name="indexProfile">The model to be deleted.</param>
    /// <returns>
    /// A <see cref="bool"/> that represents the asynchronous operation.
    /// The result is <c>true</c> if the deletion was successful, <c>false</c> otherwise.
    /// </returns>
    ValueTask<bool> DeleteAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously creates a new model with optional additional data.
    /// </summary>
    /// <param name="providerName">The provider name this model belongs to.</param>
    /// <param name="type">The type of the data source.</param>
    /// <param name="data">Optional additional data associated with the model. Defaults to <c>null</c>.</param>
    /// <returns>
    /// A <see cref="IndexProfile"/> that represents the asynchronous operation.
    /// The result is the newly created model.
    /// </returns>
    ValueTask<IndexProfile> NewAsync(string providerName, string type, JsonNode data = null);

    /// <summary>
    /// Asynchronously creates the given model.
    /// </summary>
    /// <param name="indexProfile">The model to be created.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that represents the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask CreateAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously updates the specified model with optional additional data.
    /// </summary>
    /// <param name="indexProfile">The model to be updated.</param>
    /// <param name="data">Optional additional data to update the model with. Defaults to <c>null</c>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that represents the asynchronous operation. No result is returned.
    /// </returns>
    ValueTask UpdateAsync(IndexProfile indexProfile, JsonNode data = null);

    /// <summary>
    /// Asynchronously validates the specified model.
    /// </summary>
    /// <param name="indexProfile">The model to be validated.</param>
    /// <returns>
    /// An asynchronous validation operation.
    /// The result is a <see cref="ValidationResultDetails"/> indicating whether the model is valid.
    /// </returns>
    ValueTask<ValidationResultDetails> ValidateAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously retrieves all models associated with the specified source.
    /// </summary>
    /// <param name="providerName">The source of the models. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="IEnumerable{IndexProfile}"/> representing the asynchronous operation.
    /// The result is a collection of models associated with the given source.
    /// </returns>
    ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName);

    /// <summary>
    /// Asynchronously retrieves all models associated with the specified source.
    /// </summary>
    /// <param name="providerName">The source of the models. Must not be <c>null</c> or empty.</param>
    /// <param name="type">The implementation type of the index. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="IEnumerable{IndexProfile}"/> representing the asynchronous operation.
    /// The result is a collection of models associated with the given source.
    /// </returns>
    ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type);

    /// <summary>
    /// Asynchronously synchronizes the specified index entity.
    /// </summary>
    /// <param name="indexProfile">The index entity to synchronize.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    ValueTask SynchronizeAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously resets the specified index entity.
    /// </summary>
    /// <param name="indexProfile">The index entity to reset.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    ValueTask ResetAsync(IndexProfile indexProfile);

    /// <summary>
    /// Asynchronously retrieves a model by its unique name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    ValueTask<IndexProfile> FindByNameAsync(string name);
}
