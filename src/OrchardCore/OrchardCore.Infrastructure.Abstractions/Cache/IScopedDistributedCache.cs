using System;
using System.Threading.Tasks;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// Provides a distributed cache service that can return existing references in the current scope.
    /// </summary>
    public interface IScopedDistributedCache
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
    }

    public static class SerializedCacheExtensions
    {
        public static Task<T> GetAsync<T>(this IScopedDistributedCache scopedDistributedCache)
        {
            return scopedDistributedCache.GetAsync<T>(typeof(T).FullName);
        }

        public static Task SetAsync<T>(this IScopedDistributedCache scopedDistributedCache, T value)
        {
            return scopedDistributedCache.SetAsync<T>(typeof(T).FullName, value);
        }

        public static async Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, string key, Func<Task<T>> factory)
        {
            var value = await scopedDistributedCache.GetAsync<T>(key);

            if (value == null)
            {
                value = await factory();

                await scopedDistributedCache.SetAsync(key, value);
            }

            return value;
        }

        public static Task<T> GetOrSetAsync<T>(this IScopedDistributedCache scopedDistributedCache, Func<Task<T>> factory)
        {
            return scopedDistributedCache.GetOrSetAsync(typeof(T).FullName, factory);
        }
    }
}
