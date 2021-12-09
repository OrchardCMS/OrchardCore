using System;
using System.Threading.Tasks;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A cacheable, committable and cancellable document store allowing to get documents from a shared cache while being able
    /// to update them to the store at the scope level, and allowing to register delegates that get called after committing.
    /// </summary>
    public interface IDocumentStore
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// For a full isolation, it needs to be used in pair with <see cref="GetOrCreateImmutableAsync{T}"/>.
        /// </summary>
        Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated,
        /// and a bool indicating if it can be cached, not if it has been already loaded for update.
        /// For a full isolation, it needs to be used in pair with <see cref="GetOrCreateMutableAsync{T}"/>.
        /// </summary>
        Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new();

        /// <summary>
        /// Updates the store with the provided document and then uses the delegate to update the cache.
        /// </summary>
        Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false);

        /// <summary>
        /// Allows to cancel the current updating before calling <see cref="CommitAsync"/> at the end of the scope.
        /// </summary>
        Task CancelAsync();

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitSuccessDelegate"/> that will get called after <see cref="CommitAsync"/> if it succeeded.
        /// </summary>
        void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitFailureDelegate"/> that will get called after <see cref="CommitAsync"/> if it failed.
        /// </summary>
        void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommit);

        /// <summary>
        /// Commits the related document store.
        /// </summary>
        Task CommitAsync();
    }
}
