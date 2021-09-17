using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Caching.Distributed
{
    public class MemoryDistributedCache : OrchardCore.Caching.Distributed.IDistributedCache
    {
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _underlyingCache;

        public MemoryDistributedCache(Microsoft.Extensions.Caching.Distributed.MemoryDistributedCache underlyingCache)
        {
            _underlyingCache = underlyingCache;
        }

        public byte[] Get(string key)
        {
            return _underlyingCache.Get(key);
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return _underlyingCache.GetAsync(key, token);
        }

        public Task<string> GetStringAsync(string key)
        {
            return _underlyingCache.GetStringAsync(key);
        }

        public void Refresh(string key)
        {
            _underlyingCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return _underlyingCache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            _underlyingCache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _underlyingCache.RemoveAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _underlyingCache.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return _underlyingCache.SetAsync(key, value, options, token);
        }

        public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options)
        {
            return _underlyingCache.SetStringAsync(key, value, options);
        }
    }
}
