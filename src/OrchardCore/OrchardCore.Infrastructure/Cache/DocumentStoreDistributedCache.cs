using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public class DocumentStoreDistributedCache<TDocument> : IDocumentStoreDistributedCache<TDocument> where TDocument : DistributedDocument, new()
    {
        private readonly IDocumentStore _documentStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        private TDocument _scopedCache;

        public DocumentStoreDistributedCache(IDocumentStore documentStore, IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _documentStore = documentStore;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        public DistributedCacheEntryOptions Options { get; set; }

        public async Task<TDocument> GetMutableAsync()
        {
            var document = await _documentStore.GetMutableAsync<TDocument>();

            if (_memoryCache.TryGetValue<TDocument>(typeof(TDocument).FullName, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't load for update a cached object");
            }

            document.Identifier = null;

            return document;
        }

        public async Task<TDocument> GetImmutableAsync(Func<TDocument> factory = null, bool checkConsistency = true)
        {
            var document = await GetInternalAsync();

            if (document == null)
            {
                document = await _documentStore.GetImmutableAsync(factory);

                await SetInternalAsync(document, checkConsistency);
            }

            return document;
        }

        public Task UpdateAsync(TDocument document, bool checkConcurrency = true, bool checkConsistency = true)
        {
            if (_memoryCache.TryGetValue<TDocument>(typeof(TDocument).FullName, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't update a cached object");
            }

            document.Identifier ??= IdGenerator.GenerateId();

            return _documentStore.UpdateAsync(document, document =>
            {
                return SetInternalAsync(document, checkConsistency);
            },
            checkConcurrency);
        }

        private async Task<TDocument> GetInternalAsync()
        {
            var key = typeof(TDocument).FullName;

            if (_scopedCache != null)
            {
                return _scopedCache;
            }

            var idData = await _distributedCache.GetAsync("ID_" + key);

            if (idData == null)
            {
                return null;
            }

            var id = Encoding.UTF8.GetString(idData);

            if (id == "NULL")
            {
                id = null;
            }

            if (_memoryCache.TryGetValue<TDocument>(key, out var document))
            {
                if (document.Identifier == id)
                {
                    if (Options?.SlidingExpiration.HasValue ?? false)
                    {
                        await _distributedCache.RefreshAsync(key);
                    }

                    return _scopedCache = document;
                }
            }

            var data = await _distributedCache.GetAsync(key);

            if (data == null)
            {
                return null;
            }

            using (var stream = new MemoryStream(data))
            {
                document = await DeserializeAsync(stream);
            }

            if (document.Identifier != id)
            {
                return null;
            }

            if (Options == null)
            {
                Options = new DistributedCacheEntryOptions();
            }

            _memoryCache.Set(key, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = Options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = Options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = Options.SlidingExpiration
            });

            return _scopedCache = document;
        }

        private async Task SetInternalAsync(TDocument document, bool checkConsistency)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                await SerializeAsync(stream, document);
                data = stream.ToArray();
            }

            var idData = Encoding.UTF8.GetBytes(document.Identifier ?? "NULL");

            var key = typeof(TDocument).FullName;

            if (Options == null)
            {
                Options = new DistributedCacheEntryOptions();
            }

            await _distributedCache.SetAsync(key, data, Options);
            await _distributedCache.SetAsync("ID_" + key, idData, Options);

            // Consistency: We may have been the last to update the cache but not with the last stored document.
            if (checkConsistency && (await _documentStore.GetImmutableAsync<TDocument>()).Identifier != document.Identifier)
            {
                await _distributedCache.RemoveAsync("ID_" + key);
            }
            else
            {
                _memoryCache.Set(key, document, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = Options.AbsoluteExpiration,
                    AbsoluteExpirationRelativeToNow = Options.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = Options.SlidingExpiration
                });
            }
        }

        private static Task SerializeAsync(Stream stream, TDocument document) =>
            MessagePackSerializer.SerializeAsync(stream, document, ContractlessStandardResolver.Options);

        private static ValueTask<TDocument> DeserializeAsync(Stream stream) =>
            MessagePackSerializer.DeserializeAsync<TDocument>(stream, ContractlessStandardResolver.Options);
    }
}
