using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// Provides a distributed cache service that can return existing references in the current scope.
    /// </summary>
    public interface IScopedDistributedCache
    {
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) where T : class;
    }

    public static class SerializedCacheExtensions
    {
        public static Task<T> GetAsync<T>(this IScopedDistributedCache scopedDistributedCache) where T : class
        {
            return scopedDistributedCache.GetAsync<T>(typeof(T).FullName);
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, T value) where T : class
        {
            return scopedDistributedCache.SetAsync(key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value) where T : class
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value, DistributedCacheEntryOptions options) where T : class
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, options);
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, Func<Task<T>> factory) where T : class
        {
            return scopedDistributedCache.GetOrCreateAsync(key, new DistributedCacheEntryOptions(), factory);
        }

        public static async Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, DistributedCacheEntryOptions options, Func<Task<T>> factory) where T : class
        {
            var value = await scopedDistributedCache.GetAsync<T>(key);

            if (value == null)
            {
                value = await factory();

                await scopedDistributedCache.SetAsync(key, value, options);
            }

            return value;
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, Func<Task<T>> factory) where T : class
        {
            return scopedDistributedCache.GetOrCreateAsync(typeof(T).FullName, new DistributedCacheEntryOptions(), factory);
        }

        public static Task<T> GetOrCreateAsync<T>(this IScopedDistributedCache scopedDistributedCache, DistributedCacheEntryOptions options, Func<Task<T>> factory) where T : class
        {
            return scopedDistributedCache.GetOrCreateAsync(typeof(T).FullName, options, factory);
        }
    }
}
