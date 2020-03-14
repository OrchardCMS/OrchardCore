using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrchardCore.Data;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public class DistributedDocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : DistributedDocument, new()
    {
        private readonly IDocumentStore _documentStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;

        private readonly string _key = typeof(TDocument).FullName;
        private readonly string _idKey = "ID_" + typeof(TDocument).FullName;
        private readonly DistributedCacheEntryOptions _options;

        private TDocument _scopedCache;

        public DistributedDocumentManager(
            IDocumentStore documentStore,
            IDistributedCache distributedCache,
            IShellConfiguration shellConfiguration,
            IMemoryCache memoryCache)
        {
            _documentStore = documentStore;
            _distributedCache = distributedCache;

            _options = shellConfiguration.GetSection(_key)
                .Get<DistributedCacheEntryOptions>()
                ?? new DistributedCacheEntryOptions();

            _memoryCache = memoryCache;
        }

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

        public async Task<TDocument> GetImmutableAsync(Func<TDocument> factory = null)
        {
            var test = typeof(TDocument);

            var document = await GetInternalAsync();

            if (document == null)
            {
                document = await _documentStore.GetImmutableAsync(factory);

                await SetInternalAsync(document);
            }

            return document;
        }

        public Task UpdateAsync(TDocument document, bool checkConcurrency = true)
        {
            if (_memoryCache.TryGetValue<TDocument>(typeof(TDocument).FullName, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't update a cached object");
            }

            document.Identifier ??= IdGenerator.GenerateId();

            return _documentStore.UpdateAsync(document, document =>
            {
                return SetInternalAsync(document);
            },
            checkConcurrency);
        }

        private async Task<TDocument> GetInternalAsync()
        {
            if (_scopedCache != null)
            {
                return _scopedCache;
            }

            var idData = await _distributedCache.GetAsync(_idKey);

            if (idData == null)
            {
                return null;
            }

            var id = Encoding.UTF8.GetString(idData);

            if (id == "NULL")
            {
                id = null;
            }

            if (_memoryCache.TryGetValue<TDocument>(_key, out var document))
            {
                if (document.Identifier == id)
                {
                    if (_options?.SlidingExpiration.HasValue ?? false)
                    {
                        await _distributedCache.RefreshAsync(_key);
                    }

                    return _scopedCache = document;
                }
            }

            var data = await _distributedCache.GetAsync(_key);

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

            _memoryCache.Set(_key, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = _options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _options.SlidingExpiration
            });

            return _scopedCache = document;
        }

        private async Task SetInternalAsync(TDocument document)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                await SerializeAsync(stream, document);
                data = stream.ToArray();
            }

            var idData = Encoding.UTF8.GetBytes(document.Identifier ?? "NULL");

            await _distributedCache.SetAsync(_key, data, _options);
            await _distributedCache.SetAsync(_idKey, idData, _options);

            // Consistency: We may have been the last to update the cache but not with the last stored document.
            if ((await _documentStore.GetImmutableAsync<TDocument>()).Identifier != document.Identifier)
            {
                await _distributedCache.RemoveAsync(_idKey);
            }
            else
            {
                _memoryCache.Set(_key, document, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = _options.AbsoluteExpiration,
                    AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = _options.SlidingExpiration
                });
            }
        }

        private static Task SerializeAsync(Stream stream, TDocument document) =>
            MessagePackSerializer.SerializeAsync(stream, document, ContractlessStandardResolver.Options);

        private static ValueTask<TDocument> DeserializeAsync(Stream stream) =>
            MessagePackSerializer.DeserializeAsync<TDocument>(stream, ContractlessStandardResolver.Options);
    }
}
