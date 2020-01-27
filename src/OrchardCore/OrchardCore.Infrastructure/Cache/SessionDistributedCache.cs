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
    /// <summary>
    /// A scoped service allowing to keep in sync a given data store with a multi level cache.
    /// The cache is composed of a scoped cache, a tenant level cache and a distributed cache.
    /// </summary>
    public class SessionDistributedCache : ISessionDistributedCache
    {
        private static readonly DefaultIdGenerator _idGenerator = new DefaultIdGenerator();

        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        private readonly Dictionary<string, object> _scopedCache = new Dictionary<string, object>();

        public SessionDistributedCache(IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory, DistributedCacheEntryOptions options) where T : SessionDistributedCacheEntry
        {
            var key = typeof(T).FullName;

            var value = await GetAsync<T>(key, options);

            if (value == null)
            {
                value = await factory();

                await SetAsync(key, value, options);

                _memoryCache.Set(key, value);
                _scopedCache[key] = value;
            }

            return value;
        }

        public Task UpdateAsync<T>(T value) where T : SessionDistributedCacheEntry
        {
            if (_memoryCache.TryGetValue<T>(typeof(T).FullName, out var cached) && cached == value)
            {
                throw new ArgumentException("Invalid update on a cached object");
            }

            value.CacheId = _idGenerator.GenerateUniqueId();

            return Task.CompletedTask;
        }

        public Task InvalidateAsync<T>() where T : SessionDistributedCacheEntry => _distributedCache.RemoveAsync("ID_" + typeof(T).FullName);

        public async Task RemoveAsync<T>() where T : SessionDistributedCacheEntry
        {
            var key = typeof(T).FullName;
            await _distributedCache.RemoveAsync(key);
            await _distributedCache.RemoveAsync("ID_" + key);
            _memoryCache.Remove(key);
        }

        private async Task<T> GetAsync<T>(string key, DistributedCacheEntryOptions options) where T : SessionDistributedCacheEntry
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
                    if (options.SlidingExpiration.HasValue)
                    {
                        await _distributedCache.RefreshAsync(key);
                    }

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

        private async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) where T : SessionDistributedCacheEntry
        {
            byte[] data;

            value.CacheId ??= _idGenerator.GenerateUniqueId();

            using (var ms = new MemoryStream())
            {
                await SerializeAsync(ms, value);
                data = ms.ToArray();
            }

            var cacheIdData = Encoding.UTF8.GetBytes(value.CacheId);

            await _distributedCache.SetAsync(key, data, options);
            await _distributedCache.SetAsync("ID_" + key, cacheIdData, options);
        }

        private static Task SerializeAsync<T>(Stream stream, T value) => MessagePackSerializer.SerializeAsync(stream, value, ContractlessStandardResolver.Options);

        private static ValueTask<T> DeserializeAsync<T>(Stream stream) => MessagePackSerializer.DeserializeAsync<T>(stream, ContractlessStandardResolver.Options);
    }
}
