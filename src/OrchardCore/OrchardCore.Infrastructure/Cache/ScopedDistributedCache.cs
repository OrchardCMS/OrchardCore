using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Infrastructure.Cache
{
    public class ScopedDistributedCache : IScopedDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        private readonly Dictionary<string, object> _scopedCache = new Dictionary<string, object>();

        public ScopedDistributedCache(IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            if (_scopedCache.TryGetValue(key, out var scopedValue))
            {
                return (T)scopedValue;
            }

            var data = await _distributedCache.GetAsync("Version" + key);

            if (data == null)
            {
                return null;
            }

            var version = Deserialize<int>(data);

            if (_memoryCache.TryGetValue<CacheEntry<T>>(key, out var entry))
            {
                if (entry.Version == version)
                {
                    _scopedCache[key] = entry.Value;
                    return entry.Value;
                }
            }

            data = await _distributedCache.GetAsync(key);

            if (data == null)
            {
                return null;
            }

            entry = Deserialize<CacheEntry<T>>(data);

            if (entry.Version != version)
            {
                return null;
            }

            _memoryCache.Set(key, entry);
            _scopedCache[key] = entry.Value;

            return entry.Value;
        }

        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) where T : class
        {
            var versionData = await _distributedCache.GetAsync("Version" + key);
            var version = versionData != null ? Deserialize<int>(versionData) : new Random().Next();

            versionData = Serialize(++version);
            var entry = new CacheEntry<T>() { Version = version, Value = value };
            var data = Serialize(entry);

            await _distributedCache.SetAsync("Version" + key, versionData, options);
            await _distributedCache.SetAsync(key, data);
            _memoryCache.Set(key, entry);
        }

        private byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value, ContractlessStandardResolver.Options);
        }

        private T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data, ContractlessStandardResolver.Options);
        }

        public class CacheEntry<T>
        {
            public int Version { get; set; }
            public T Value { get; set; }
        }
    }
}
