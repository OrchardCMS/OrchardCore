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
        Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory, DistributedCacheEntryOptions options) where T : SessionDistributedCacheEntry;
        Task UpdateAsync<T>(T value) where T : SessionDistributedCacheEntry;
        Task InvalidateAsync<T>() where T : SessionDistributedCacheEntry;
        Task RemoveAsync<T>() where T : SessionDistributedCacheEntry;
    }
}
