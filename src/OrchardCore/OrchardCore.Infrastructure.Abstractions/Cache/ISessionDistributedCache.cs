using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A scoped service allowing to keep in sync a given data store with a multi level cache.
    /// The cache is composed of a scoped cache, a tenant level cache and a distributed cache.
    /// </summary>
    public interface ISessionDistributedCache
    {
        Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory, DistributedCacheEntryOptions options) where T : DistributedCacheData;
        Task SetAsync<T>(T value, DistributedCacheEntryOptions options) where T : DistributedCacheData;
        Task<bool> HasChangedAsync<T>(T value) where T : DistributedCacheData;
        Task<bool> TryUpdateAsync<T>(T value) where T : DistributedCacheData;
        Task InvalidateAsync<T>() where T : DistributedCacheData;
        Task RemoveAsync<T>() where T : DistributedCacheData;
    }
}
