using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;
using OrchardCore.Documents.Options;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocument"/> between an <see cref="IDocumentStore"/> and a multi level cache.
    /// </summary>
    public class DocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        private readonly IDocumentStore _documentStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly DocumentOptions _options;

        private TDocument _scopedCache;
        private TDocument _volatileCache;
        private readonly bool _isDistributed;
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

            if (!(_distributedCache is MemoryDistributedCache))
            {
                _isDistributed = true;
            }
        }

        public async Task<TDocument> GetOrCreateMutableAsync(Func<Task<TDocument>> factoryAsync = null)
        {
            TDocument document = null;

            if (!_isVolatile)
            {
                document = await _documentStore.GetOrCreateMutableAsync(factoryAsync);

                if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
                {
                    throw new InvalidOperationException("Can't load for update a cached object");
                }
            }
            else
            {
                if (_volatileCache != null)
                {
                    return _volatileCache;
                }

                _volatileCache = document = await GetFromDistributedCacheAsync()
                    ?? await (factoryAsync?.Invoke() ?? Task.FromResult((TDocument)null))
                    ?? new TDocument();
            }

            document.Identifier = null;

            return document;
        }

        public async Task<TDocument> GetOrCreateImmutableAsync(Func<Task<TDocument>> factoryAsync = null)
        {
            var document = await GetInternalAsync();

            if (document == null)
            {
                var cacheable = true;

                if (!_isVolatile)
                {
                    (cacheable, document) = await _documentStore.GetOrCreateImmutableAsync(factoryAsync);
                }
                else
                {
                    document = await (factoryAsync?.Invoke()
                        ?? Task.FromResult((TDocument)null))
                        ?? new TDocument();
                }

                if (cacheable)
                {
                    await SetInternalAsync(document);
                }
            }

            return document;
        }

        public Task UpdateAsync(TDocument document)
        {
            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
            {
                throw new InvalidOperationException("Can't update a cached object");
            }

            document.Identifier ??= IdGenerator.GenerateId();

            if (!_isVolatile)
            {
                return _documentStore.UpdateAsync(document, document =>
                {
                    return SetInternalAsync(document);
                },
                _options.CheckConcurrency.Value);
            }

            // Set the scoped cache in case of multiple updates.
            _volatileCache = document;

            // But still update the shared cache after committing.
            _documentStore.AfterCommitSuccess<TDocument>(() =>
            {
                return SetInternalAsync(document);
            });

            return Task.CompletedTask;
        }

        private async Task<TDocument> GetInternalAsync()
        {
            if (_scopedCache != null)
            {
                return _scopedCache;
            }

            string id;
            if (_isDistributed)
            {
                // Cache the id locally for one second to prevent network contention.
                id = await _memoryCache.GetOrCreateAsync(_options.CacheIdKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                    return _distributedCache.GetStringAsync(_options.CacheIdKey);
                });
            }
            else
            {
                // Otherwise, always get the id from the in memory distributed cache.
                id = await _distributedCache.GetStringAsync(_options.CacheIdKey);
            }

            if (id == null)
            {
                return null;
            }

            if (id == "NULL")
            {
                id = null;
            }

            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var document))
            {
                if (document.Identifier == id)
                {
                    if (_isDistributed && (_options?.SlidingExpiration.HasValue ?? false))
                    {
                        await _distributedCache.RefreshAsync(_options.CacheKey);
                    }

                    return _scopedCache = document;
                }
            }

            if (!_isDistributed)
            {
                return null;
            }

            document = await GetFromDistributedCacheAsync();

            if (document == null)
            {
                return null;
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

            if (_isDistributed)
            {
                // Remove the id from the one second cache.
                _memoryCache.Remove(_options.CacheIdKey);
            }

            return _scopedCache = document;
        }

        private async Task SetInternalAsync(TDocument document)
        {
            await UpdateDistributedCacheAsync(document);

            _memoryCache.Set(_options.CacheKey, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = _options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _options.SlidingExpiration
            });

            if (_isDistributed)
            {
                // Remove the id from the one second cache.
                _memoryCache.Remove(_options.CacheIdKey);
            }

            // Consistency: We may have been the last to update the cache but not with the last stored document.
            if (!_isVolatile && _options.CheckConsistency.Value)
            {
                (_, var stored) = await _documentStore.GetOrCreateImmutableAsync<TDocument>();

                if (stored.Identifier != document.Identifier)
                {
                    await _distributedCache.RemoveAsync(_options.CacheIdKey);
                }
            }
        }

        private async Task<TDocument> GetFromDistributedCacheAsync()
        {
            byte[] data = null;

            if (_isDistributed)
            {
                data = await _distributedCache.GetAsync(_options.CacheKey);
            }
            else if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached))
            {
                data = Serialize(cached);
            }

            if (data == null)
            {
                return null;
            }

            return Deserialize(data);
        }

        private async Task UpdateDistributedCacheAsync(TDocument document)
        {
            if (_isDistributed)
            {
                var data = Serialize(document);
                await _distributedCache.SetAsync(_options.CacheKey, data, _options);
            }

            await _distributedCache.SetStringAsync(_options.CacheIdKey, document.Identifier ?? "NULL", _options);
        }

        internal static byte[] Serialize(TDocument document) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(document, _jsonSettings));

        internal static TDocument Deserialize(byte[] data) =>
            JsonConvert.DeserializeObject<TDocument>(Encoding.UTF8.GetString(data), _jsonSettings);
    }
}
