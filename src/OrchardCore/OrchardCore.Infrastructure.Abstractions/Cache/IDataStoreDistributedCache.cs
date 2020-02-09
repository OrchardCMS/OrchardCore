using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Data;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic scoped service to keep in sync a given data store with a multi level
    /// cache composed of a scoped cache, a tenant level cache and a distributed cache.
    /// </summary>
    public interface IDataStoreDistributedCache<TDataStore> where TDataStore : ICacheableDataStore
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
        /// Updates the cache, intended to be called after having updated the data store.
        /// </summary>
        Task UpdateAsync<T>(T value, DistributedCacheEntryOptions options = null) where T : DistributedCacheData, new();

        /// <summary>
        /// Checks if the <see cref="DistributedCacheData.Identifier"/> of the cached value has changed.
        /// </summary>
        Task<bool> HasChangedAsync<T>(T value) where T : DistributedCacheData;

        /// <summary>
        /// Invalidates the cache.
        /// </summary>
        Task InvalidateAsync<T>() where T : DistributedCacheData;
    }
}
