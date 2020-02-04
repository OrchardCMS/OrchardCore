using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A scoped service allowing to keep in sync a given data store with a multi level
    /// cache composed of a scoped cache, a tenant memory cache and a distributed cache.
    /// </summary>
    public interface IScopedDistributedCache
    {
        Task<T> LoadAsync<T>(Func<Task<T>> loadForUpdate) where T : ScopedDistributedCacheData;
        Task<T> GetAsync<T>(Func<Task<T>> getForCachingAsync, DistributedCacheEntryOptions options) where T : ScopedDistributedCacheData;
        Task UpdateAsync<T>(T value, DistributedCacheEntryOptions options) where T : ScopedDistributedCacheData;
        Task<bool> HasChangedAsync<T>(T value) where T : ScopedDistributedCacheData;
        Task InvalidateAsync<T>() where T : ScopedDistributedCacheData;
    }
}
