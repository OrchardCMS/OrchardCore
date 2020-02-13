using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data;
using OrchardCore.Entities;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public class DistributedCache<TDocumentStore> : IDistributedCache<TDocumentStore> where TDocumentStore : ICacheableDocumentStore
    {
        private static readonly DefaultIdGenerator _idGenerator = new DefaultIdGenerator();

        private readonly TDocumentStore _documentStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        private readonly Dictionary<string, object> _scopedCache = new Dictionary<string, object>();

        public DistributedCache(TDocumentStore documentStore, IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _documentStore = documentStore;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        public async Task<T> LoadAsync<T>() where T : DistributedDocument, new()
        {
            var document = await _documentStore.LoadForUpdateAsync<T>();

            if (_memoryCache.TryGetValue<T>(typeof(T).FullName, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't load for update a cached object");
            }

            document.Identifier = _idGenerator.GenerateUniqueId();

            return document;
        }

        public async Task<T> GetAsync<T>(Func<T> factory = null, DistributedCacheEntryOptions options = null, bool checkConsistency = true) where T : DistributedDocument, new()
        {
            if (options == null)
            {
                options = new DistributedCacheEntryOptions();
            }

            var document = await GetInternalAsync<T>(options);

            if (document == null)
            {
                document = await _documentStore.GetForCachingAsync(factory);

                await SetInternalAsync(document, options, checkConsistency);
            }

            return document;
        }

        public Task UpdateAsync<T>(T document, DistributedCacheEntryOptions options = null, bool checkConcurrency = true, bool checkConsistency = true) where T : DistributedDocument, new()
        {
            if (_memoryCache.TryGetValue<T>(typeof(T).FullName, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't update a cached object");
            }

            if (options == null)
            {
                options = new DistributedCacheEntryOptions();
            }

            return _documentStore.UpdateAsync(document, document =>
            {
                return SetInternalAsync(document, options, checkConsistency);
            },
            checkConcurrency);
        }

        private async Task<T> GetInternalAsync<T>(DistributedCacheEntryOptions options) where T : DistributedDocument
        {
            var key = typeof(T).FullName;

            if (_scopedCache.TryGetValue(key, out var scoped))
            {
                return (T)scoped;
            }

            var idData = await _distributedCache.GetAsync("ID_" + key);

            if (idData == null)
            {
                return null;
            }

            var id = Encoding.UTF8.GetString(idData);

            if (_memoryCache.TryGetValue<T>(key, out var document))
            {
                if (document.Identifier == id)
                {
                    if (options.SlidingExpiration.HasValue)
                    {
                        await _distributedCache.RefreshAsync(key);
                    }

                    _scopedCache[key] = document;

                    return document;
                }
            }

            var data = await _distributedCache.GetAsync(key);

            if (data == null)
            {
                return null;
            }

            using (var stream = new MemoryStream(data))
            {
                document = await DeserializeAsync<T>(stream);
            }

            if (document.Identifier != id)
            {
                return null;
            }

            _memoryCache.Set(key, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            });

            _scopedCache[key] = document;

            return document;
        }

        private async Task SetInternalAsync<T>(T document, DistributedCacheEntryOptions options, bool checkConsistency) where T : DistributedDocument, new()
        {
            document.Identifier ??= _idGenerator.GenerateUniqueId();

            byte[] data;

            using (var stream = new MemoryStream())
            {
                await SerializeAsync(stream, document);
                data = stream.ToArray();
            }

            var idData = Encoding.UTF8.GetBytes(document.Identifier);

            var key = typeof(T).FullName;

            await _distributedCache.SetAsync(key, data, options);
            await _distributedCache.SetAsync("ID_" + key, idData, options);

            // In case we were the last to update the cache, check if we didn't cache the last stored document.
            if (checkConsistency && (await _documentStore.GetForCachingAsync<T>()).Identifier != document.Identifier)
            {
                await _distributedCache.RemoveAsync("ID_" + key);
            }
            else
            {
                _memoryCache.Set(key, document, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = options.AbsoluteExpiration,
                    AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = options.SlidingExpiration
                });

                if (!_scopedCache.TryGetValue(key, out var scoped))
                {
                    _scopedCache[key] = document;
                }
            }
        }

        private static Task SerializeAsync<T>(Stream stream, T document) =>
            MessagePackSerializer.SerializeAsync(stream, document, ContractlessStandardResolver.Options);

        private static ValueTask<T> DeserializeAsync<T>(Stream stream) =>
            MessagePackSerializer.DeserializeAsync<T>(stream, ContractlessStandardResolver.Options);
    }
}
