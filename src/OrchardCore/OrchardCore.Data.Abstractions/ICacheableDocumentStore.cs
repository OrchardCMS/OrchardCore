using System;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// Allows to get documents from a shared cache while being able to update them to the store at the scope level.
    /// </summary>
    public interface ICacheableDocumentStore
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// For a full isolation, it needs to be used in pair with <see cref="GetForCachingAsync"/>.
        /// </summary>
        Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// For a full isolation, it needs to be used in pair with <see cref="LoadForUpdateAsync"/>.
        /// </summary>
        Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Updates the store with the provided document and then uses the delegate to update the cache.
        /// </summary>
        Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false);
    }
}
