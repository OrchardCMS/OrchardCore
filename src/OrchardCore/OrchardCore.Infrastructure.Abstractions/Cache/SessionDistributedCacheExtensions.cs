using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    public static class SessionDistributedCacheExtensions
    {
        public static Task<T> GetOrCreateAsync<T>(this ISessionDistributedCache sessionDistributedCache, Func<Task<T>> factory) where T : DistributedCacheData
        {
            return sessionDistributedCache.GetOrCreateAsync<T>(factory, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this ISessionDistributedCache sessionDistributedCache, T value) where T : DistributedCacheData
        {
            return sessionDistributedCache.SetAsync(value, new DistributedCacheEntryOptions());
        }
    }
}
