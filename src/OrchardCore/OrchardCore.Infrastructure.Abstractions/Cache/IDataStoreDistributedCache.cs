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
        Task<T> LoadAsync<T>() where T : DistributedCacheData, new();
        Task<T> GetAsync<T>(Func<T> factory = null, DistributedCacheEntryOptions options = null) where T : DistributedCacheData, new();
        Task UpdateAsync<T>(T value, DistributedCacheEntryOptions options = null) where T : DistributedCacheData, new();
        Task<bool> HasChangedAsync<T>(T value) where T : DistributedCacheData;
        Task InvalidateAsync<T>() where T : DistributedCacheData;
    }
}
