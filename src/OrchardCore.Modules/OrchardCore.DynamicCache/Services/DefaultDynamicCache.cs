using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.DynamicCache.Services
{
    public class DefaultDynamicCache : IDynamicCache
    {
        private readonly IDistributedCache _distributedCache;

        public DefaultDynamicCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public Task<byte[]> GetAsync(string key)
        {
            return _distributedCache.GetAsync(key);
        }

        public Task RemoveAsync(string key)
        {
            return _distributedCache.RemoveAsync(key);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            return _distributedCache.SetAsync(key, value, options);
        }
    }
}
