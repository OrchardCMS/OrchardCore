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

        public async Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory, DistributedCacheEntryOptions options) where T : DistributedCacheData
        {
            var value = await GetAsync<T>(options);

            if (value == null)
            {
                value = await factory();
                await SetAsync(value, options);
            }

            return value;
        }

        private async Task<T> GetAsync<T>(DistributedCacheEntryOptions options) where T : DistributedCacheData
        {
            var key = typeof(T).FullName;

            if (_scopedCache.TryGetValue(key, out var scopedValue))
            {
                return (T)scopedValue;
            }

            var idData = await _distributedCache.GetAsync("ID_" + key);

            if (idData == null)
            {
                return null;
            }

            var id = Encoding.UTF8.GetString(idData);

            if (_memoryCache.TryGetValue<T>(key, out var value))
            {
                if (value.Identifier == id)
                {
                    if (value.HasSlidingExpiration)
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

            if (value.Identifier != id)
            {
                return null;
            }

            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            });

            _scopedCache[key] = value;

            return value;
        }

        public async Task SetAsync<T>(T value, DistributedCacheEntryOptions options) where T : DistributedCacheData
        {
            value.Identifier ??= _idGenerator.GenerateUniqueId();

            if (options.SlidingExpiration.HasValue)
            {
                value.HasSlidingExpiration = true;
            }

            byte[] data;

            using (var ms = new MemoryStream())
            {
                await SerializeAsync(ms, value);
                data = ms.ToArray();
            }

            var idData = Encoding.UTF8.GetBytes(value.Identifier);

            var key = typeof(T).FullName;

            await _distributedCache.SetAsync(key, data, options);
            await _distributedCache.SetAsync("ID_" + key, idData, options);

            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            });

            _scopedCache[key] = value;
        }

        public async Task<bool> HasChangedAsync<T>(T value) where T : DistributedCacheData
        {
            var key = typeof(T).FullName;

            var idData = await _distributedCache.GetAsync("ID_" + key);

            if (idData == null)
            {
                return true;
            }

            var id = Encoding.UTF8.GetString(idData);

            return value.Identifier != id;
        }

        public Task<bool> TryUpdateAsync<T>(T value) where T : DistributedCacheData
        {
            if (_memoryCache.TryGetValue<T>(typeof(T).FullName, out var cached) && value == cached)
            {
                return Task.FromResult(false);
            }

            value.Identifier = _idGenerator.GenerateUniqueId();

            return Task.FromResult(true);
        }

        public Task InvalidateAsync<T>() where T : DistributedCacheData => _distributedCache.RemoveAsync("ID_" + typeof(T).FullName);

        public async Task RemoveAsync<T>() where T : DistributedCacheData
        {
            var key = typeof(T).FullName;
            await _distributedCache.RemoveAsync(key);
            await _distributedCache.RemoveAsync("ID_" + key);
            _memoryCache.Remove(key);
        }

        private static Task SerializeAsync<T>(Stream stream, T value) => MessagePackSerializer.SerializeAsync(stream, value, ContractlessStandardResolver.Options);

        private static ValueTask<T> DeserializeAsync<T>(Stream stream) => MessagePackSerializer.DeserializeAsync<T>(stream, ContractlessStandardResolver.Options);
    }
}
