using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    public static class ScopedDistributedCacheExtensions
    {
        public static Task<T> GetAsync<T>(this IScopedDistributedCache sessionDistributedCache, Func<Task<T>> factory) where T : ScopedDistributedCacheData
        {
            return sessionDistributedCache.GetAsync<T>(factory, new DistributedCacheEntryOptions());
        }

        public static Task UpdateAsync<T>(this IScopedDistributedCache sessionDistributedCache, T value) where T : ScopedDistributedCacheData
        {
            return sessionDistributedCache.UpdateAsync(value, new DistributedCacheEntryOptions());
        }
    }
}
