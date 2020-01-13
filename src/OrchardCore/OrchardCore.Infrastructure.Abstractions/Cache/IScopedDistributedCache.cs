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
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options);
    }

    public static class SerializedCacheExtensions
    {
        public static Task<T> GetAsync<T>(this IScopedDistributedCache scopedDistributedCache)
        {
            return scopedDistributedCache.GetAsync<T>(typeof(T).FullName);
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, T value)
        {
            return scopedDistributedCache.SetAsync(key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value)
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value, DistributedCacheEntryOptions options)
        {
            return scopedDistributedCache.SetAsync(typeof(T).FullName, value, options);
        }

        public static Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, Func<Task<T>> factory)
        {
            return scopedDistributedCache.GetOrSetAsync(key, new DistributedCacheEntryOptions(), factory);
        }

        public static async Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, DistributedCacheEntryOptions options, Func<Task<T>> factory)
        {
            var value = await scopedDistributedCache.GetAsync<T>(key);

            if (value == null)
            {
                value = await factory();

                await scopedDistributedCache.SetAsync(key, value, options);
            }

            return value;
        }

        public static Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, Func<Task<T>> factory)
        {
            return scopedDistributedCache.GetOrSetAsync(typeof(T).FullName, new DistributedCacheEntryOptions(), factory);
        }

        public static Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, DistributedCacheEntryOptions options, Func<Task<T>> factory)
        {
            return scopedDistributedCache.GetOrSetAsync(typeof(T).FullName, options, factory);
        }
    }
}
