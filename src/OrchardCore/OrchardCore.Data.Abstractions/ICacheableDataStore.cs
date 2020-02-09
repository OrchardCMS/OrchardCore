using System;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// Allows to update the data of a given data store while being able to retrieve them through a shared cached />.
    /// </summary>
    public interface ICacheableDataStore
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
        /// Updates the store with the provided value and then uses the delegate to update the cache.
        /// </summary>
        Task UpdateAsync<T>(T value, Func<T, Task> updateCache) where T : class, new();
    }
}
