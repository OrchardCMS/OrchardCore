using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Entities;

namespace OrchardCore.Infrastructure.Cache
{
    public class ScopedDistributedCache : IScopedDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly IIdGenerator _idGenerator;

        private readonly Dictionary<string, object> _scopedCache = new Dictionary<string, object>();

        public ScopedDistributedCache(IDistributedCache distributedCache, IMemoryCache memoryCache, IIdGenerator idGenerator)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _idGenerator = idGenerator;
        }

        public async Task<T> GetAsync<T>(string key) where T : class, IScopedDistributedCacheable
        {
            if (_scopedCache.TryGetValue(key, out var scopedValue))
            {
                return (T)scopedValue;
            }

            var cacheIdData = await _distributedCache.GetAsync("ID_" + key);

            if (cacheIdData == null)
            {
                return null;
            }

            var cacheId = Encoding.UTF8.GetString(cacheIdData);

            if (_memoryCache.TryGetValue<T>(key, out var value))
            {
                if (value.CacheId == cacheId)
                {
                    _scopedCache[key] = value;
                    return value;
                }
            }

            var data = await _distributedCache.GetAsync(key);

            if (data == null)
            {
                return null;
            }

            using (var ms = new MemoryStream(data))
            {
                value = await DeserializeAsync<T>(ms);
            }

            if (value.CacheId != cacheId)
            {
                return null;
            }

            _memoryCache.Set(key, value);
            _scopedCache[key] = value;

            return value;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T>> factory) where T : class, IScopedDistributedCacheable
        {
            var value = await GetAsync<T>(key);

            if (value == null)
            {
                value = await factory();

                await SyncAsync(key, value, options);

                _memoryCache.Set(key, value);
                _scopedCache[key] = value;
            }

            return value;
        }

        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) where T : class, IScopedDistributedCacheable
        {
            if (value == null)
            {
                return Task.CompletedTask;
            }

            value.CacheId = _idGenerator.GenerateUniqueId();
            return SetAsyncInternal(key, value, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync("ID_" + key);
            await _distributedCache.RemoveAsync(key);
        }

        private Task SyncAsync<T>(string key, T value, DistributedCacheEntryOptions options) where T : class, IScopedDistributedCacheable
        {
            value.CacheId ??= _idGenerator.GenerateUniqueId();
            return SetAsyncInternal(key, value, options);
        }

        private async Task SetAsyncInternal<T>(string key, T value, DistributedCacheEntryOptions options) where T : class, IScopedDistributedCacheable
        {
            if (value.CacheId == null)
            {
                throw new ArgumentNullException(nameof(value.CacheId));
            }

            byte[] data;

            using (var ms = new MemoryStream())
            {
                await SerializeAsync(ms, value);
                data = ms.ToArray();
            }

            var id = Encoding.UTF8.GetBytes(value.CacheId);

            await _distributedCache.SetAsync(key, data, options);
            await _distributedCache.SetAsync("ID_" + key, id, options);
        }

        private Task SerializeAsync<T>(Stream stream, T value) => MessagePackSerializer.SerializeAsync(stream, value, ContractlessStandardResolver.Options);

        private ValueTask<T> DeserializeAsync<T>(Stream stream) => MessagePackSerializer.DeserializeAsync<T>(stream, ContractlessStandardResolver.Options);
    }
}
