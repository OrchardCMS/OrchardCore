using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocument"/> between an <see cref="IDocumentStore"/> and a multi level cache.
    /// </summary>
    public class DocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        private readonly IDocumentStore _documentStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly DocumentOptions _options;

        private TDocument _scopedCache;
        private TDocument _scopedMutable;
        protected bool _isVolatile;

        public DocumentManager(
            IDocumentStore documentStore,
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            DocumentOptions<TDocument> options)
        {
            _documentStore = documentStore;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public async Task<TDocument> GetMutableAsync(Func<TDocument> factory = null)
        {
            TDocument document = null;

            if (_isVolatile)
            {
                // For volatile data, we deserialize the document from the distributed cache so that it can
                // be safely updated, and we hold a scoped cache as it would be done by the document store.

                if (_scopedMutable != null)
                {
                    return _scopedMutable;
                }

                var data = await _distributedCache.GetAsync(_options.CacheKey);

                if (data != null)
                {
                    using (var stream = new MemoryStream(data))
                    {
                        document = await DeserializeAsync(stream);
                    }
                }

                document ??= factory?.Invoke() ?? new TDocument();

                document.Identifier = null;

                return _scopedMutable = document;
            }

            document = await _documentStore.GetMutableAsync(factory);

            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't load for update a cached object");
            }

            document.Identifier = null;

            return document;
        }

        public async Task<TDocument> GetImmutableAsync(Func<TDocument> factory = null)
        {
            var document = await GetInternalAsync();

            if (document == null)
            {
                if (_isVolatile)
                {
                    document = factory?.Invoke() ?? new TDocument();
                }
                else
                {
                    document = await _documentStore.GetImmutableAsync(factory);
                }

                await SetInternalAsync(document);
            }

            return document;
        }

        public Task UpdateAsync(TDocument document)
        {
            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
            {
                throw new ArgumentException("Can't update a cached object");
            }

            document.Identifier ??= IdGenerator.GenerateId();

            if (_isVolatile)
            {
                // Still update the cache after committing.
                _documentStore.AfterCommitSuccess<TDocument>(() =>
                {
                    return SetInternalAsync(document);
                });

                return Task.CompletedTask;
            }

            return _documentStore.UpdateAsync(document, document =>
            {
                return SetInternalAsync(document);
            },
            _options.CheckConcurrency.Value);
        }

        private async Task<TDocument> GetInternalAsync()
        {
            if (_scopedCache != null)
            {
                return _scopedCache;
            }

            var idData = await _distributedCache.GetAsync(_options.CacheIdKey);

            if (idData == null)
            {
                return null;
            }

            var id = Encoding.UTF8.GetString(idData);

            if (id == "NULL")
            {
                id = null;
            }

            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var document))
            {
                if (document.Identifier == id)
                {
                    if (_options?.SlidingExpiration.HasValue ?? false)
                    {
                        await _distributedCache.RefreshAsync(_options.CacheKey);
                    }

                    return _scopedCache = document;
                }
            }

            var data = await _distributedCache.GetAsync(_options.CacheKey);

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

            _memoryCache.Set(_options.CacheKey, document, new MemoryCacheEntryOptions()
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

            await _distributedCache.SetAsync(_options.CacheKey, data, _options);
            await _distributedCache.SetAsync(_options.CacheIdKey, idData, _options);

            _memoryCache.Set(_options.CacheKey, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = _options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _options.SlidingExpiration
            });

            if (_isVolatile)
            {
                return;
            }

            // Consistency: We may have been the last to update the cache but not with the last stored document.
            if (_options.CheckConsistency.Value && (await _documentStore.GetImmutableAsync<TDocument>()).Identifier != document.Identifier)
            {
                await _distributedCache.RemoveAsync(_options.CacheIdKey);
            }
        }

        private static Task SerializeAsync(Stream stream, TDocument document) =>
            MessagePackSerializer.SerializeAsync(stream, document, ContractlessStandardResolver.Options);

        private static ValueTask<TDocument> DeserializeAsync(Stream stream) =>
            MessagePackSerializer.DeserializeAsync<TDocument>(stream, ContractlessStandardResolver.Options);
    }
}
