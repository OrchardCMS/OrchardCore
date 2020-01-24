using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    public static class ScopedDistributedCacheExtensions
    {
        public static Task<T> GetAsync<T>(this IScopedDistributedCache scopedDistributedCache) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.GetAsync<T>(typeof(T).FullName);
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, T value) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.SetAsync(key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value, DistributedCacheEntryOptions options) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, options);
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, Func<Task<T>> factory) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.GetOrCreateAsync(key, new DistributedCacheEntryOptions(), factory);
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, Func<Task<T>> factory) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.GetOrCreateAsync(typeof(T).FullName, new DistributedCacheEntryOptions(), factory);
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, DistributedCacheEntryOptions options, Func<Task<T>> factory) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.GetOrCreateAsync(typeof(T).FullName, options, factory);
        }

        public static Task RemoveAsync<T>(this IScopedDistributedCache scopedDistributedCache) where T : ScopedDistributedCacheEntry
        {
            return scopedDistributedCache.RemoveAsync(typeof(T).FullName);
        }
    }
}
