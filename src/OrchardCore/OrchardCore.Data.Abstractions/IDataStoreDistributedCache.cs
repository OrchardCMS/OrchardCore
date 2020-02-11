using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Data
{
    /// <summary>
    /// A scoped service to keep in sync a given data store with a multi level cache composed of
    /// a scoped level cache, a tenant level memory cache and a tenant level distributed cache.
    /// </summary>
    public interface IDataStoreDistributedCache
    {
        /// <summary>
        /// Loads a single document from the store or create a new one by using <see cref="ICacheableDataStore.LoadForUpdateAsync"/>.
        /// </summary>
        Task<T> LoadAsync<T>() where T : DistributedCacheData, new();

        /// <summary>
        /// Gets a single document from the cache or create a new one by using <see cref="ICacheableDataStore.GetForCachingAsync"/>.
        /// </summary>
        Task<T> GetAsync<T>(Func<T> factory = null, DistributedCacheEntryOptions options = null) where T : DistributedCacheData, new();

        /// <summary>
        /// Updates the store with the provided value and then updates the cache.
        /// </summary>
        Task UpdateAsync<T>(T value, DistributedCacheEntryOptions options = null) where T : DistributedCacheData, new();
    }
}
