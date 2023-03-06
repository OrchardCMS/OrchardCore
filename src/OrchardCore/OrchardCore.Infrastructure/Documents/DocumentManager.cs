using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Documents;
using OrchardCore.Documents.Options;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocument"/> between an <see cref="IDocumentStore"/> and a multi level cache.
    /// </summary>
    public class DocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        public const string FailoverKey = "OrchardCore_Documents_FailoverKey";

        protected readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        protected readonly DocumentOptions _options;
        private readonly ILogger _logger;

        protected readonly bool _isDistributed;
        protected bool _isVolatile;

        public DocumentManager(
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            IOptionsMonitor<DocumentOptions> options,
            ILogger<DocumentManager<TDocument>> logger)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _options = options.Get(typeof(TDocument).FullName);
            _logger = logger;

            if (_distributedCache is not MemoryDistributedCache)
            {
                _isDistributed = true;
            }

            DocumentStoreServiceType = typeof(IDocumentStore);
        }

        public Type DocumentStoreServiceType { get; set; }

        public IDocumentStore DocumentStore
        {
            get
            {
                var documentStore = (IDocumentStore)ShellScope.Get(DocumentStoreServiceType);
                if (documentStore == null)
                {
                    documentStore = (IDocumentStore)ShellScope.Services.GetRequiredService(DocumentStoreServiceType);
                    ShellScope.Set(DocumentStoreServiceType, documentStore);
                }

                return documentStore;
            }
        }

        public async Task<TDocument> GetOrCreateMutableAsync(Func<Task<TDocument>> factoryAsync = null)
        {
            TDocument document = null;

            if (!_isVolatile)
            {
                document = await DocumentStore.GetOrCreateMutableAsync(factoryAsync);

                if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
                {
                    throw new InvalidOperationException("Can't load for update a cached object");
                }
            }
            else
            {
                var volatileCache = ShellScope.Get<TDocument>(typeof(TDocument));
                if (volatileCache != null)
                {
                    document = volatileCache;
                }
                else
                {
                    document = await GetFromDistributedCacheAsync()
                        ?? await (factoryAsync?.Invoke() ?? Task.FromResult((TDocument)null))
                        ?? new TDocument();

                    ShellScope.Set(typeof(TDocument), document);
                }
            }

            document.Identifier = IdGenerator.GenerateId();

            return document;
        }

        public async Task<TDocument> GetOrCreateImmutableAsync(Func<Task<TDocument>> factoryAsync = null)
        {
            // If called from a constructor, e.g. through an 'IConfigureOptions', the 'IDocumentStore' should
            // be resolved before any async IO operation to prevent a DI dead lock. So, only if the store may
            // be used (if non volatile) and an async IO may be done before (if a distributed cache is used).

            if (!_isVolatile && _isDistributed)
            {
                // Resolve the 'IDocumentStore' and hold it in a scoped cache.
                _ = DocumentStore;
            }

            TDocument document = null;

            var failover = _isDistributed && _memoryCache.Get<bool>(FailoverKey);
            try
            {
                // May call an async IO if using a distributed cache.
                document = await GetInternalAsync(failover);
            }
            catch (Exception e)
            {
                if (!_isDistributed)
                {
                    throw;
                }

                _logger.LogError(e, "Failed to read the '{DocumentName}' from the distributed cache", typeof(TDocument).Name);

                failover = true;
                _memoryCache.Set(FailoverKey, failover, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = _options.FailoverRetryLatency
                });
            }

            if (document == null)
            {
                var cacheable = true;

                if (!_isVolatile)
                {
                    (cacheable, document) = await DocumentStore.GetOrCreateImmutableAsync(factoryAsync);
                }
                else
                {
                    document = await (factoryAsync?.Invoke()
                        ?? Task.FromResult((TDocument)null))
                        ?? new TDocument();
                }

                if (cacheable)
                {
                    await SetInternalAsync(document, failover);
                }
            }

            return document;
        }

        public async Task UpdateAsync(TDocument document, Func<TDocument, Task> afterUpdateAsync = null)
        {
            if (_isDistributed)
            {
                try
                {
                    _ = await _distributedCache.GetStringAsync(_options.CacheIdKey);
                }
                catch
                {
                    await DocumentStore.CancelAsync();

                    _logger.LogError("Can't update the '{DocumentName}' if not able to access the distributed cache", typeof(TDocument).Name);

                    throw;
                }
            }

            if (_memoryCache.TryGetValue<TDocument>(_options.CacheKey, out var cached) && document == cached)
            {
                throw new InvalidOperationException("Can't update a cached object");
            }

            document.Identifier ??= IdGenerator.GenerateId();

            if (!_isVolatile)
            {
                await DocumentStore.UpdateAsync(document, async document =>
                {
                    await SetInternalAsync(document);

                    if (afterUpdateAsync != null)
                    {
                        await afterUpdateAsync(document);
                    }
                },
                _options.CheckConcurrency.Value);

                return;
            }

            // Set the scoped cache in case of multiple updates.
            ShellScope.Set(typeof(TDocument), document);

            // But still update the shared cache after committing.
            DocumentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                await SetInternalAsync(document);

                if (afterUpdateAsync != null)
                {
                    await afterUpdateAsync(document);
                }
            });

            return;
        }

        private async Task<TDocument> GetInternalAsync(bool failover = false)
        {
            string id;
            if (_isDistributed)
            {
                // Cache the id locally for the synchronization latency time.
                id = await _memoryCache.GetOrCreateAsync(_options.CacheIdKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _options.SynchronizationLatency;

                    if (failover)
                    {
                        return Task.FromResult<string>(null);
                    }

                    return _distributedCache.GetStringAsync(_options.CacheIdKey);
                });
            }
            else
            {
                // Otherwise, always get the id from the memory cache.
                id = _memoryCache.Get<string>(_options.CacheIdKey);
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
                    if (failover)
                    {
                        return document;
                    }

                    if (_isDistributed && (_options?.SlidingExpiration.HasValue ?? false))
                    {
                        await _distributedCache.RefreshAsync(_options.CacheKey);
                    }

                    return document;
                }
            }

            if (!_isDistributed)
            {
                return null;
            }

            if (failover)
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

            // Remove the id from the one second cache.
            _memoryCache.Remove(_options.CacheIdKey);

            return document;
        }

        protected async Task SetInternalAsync(TDocument document, bool failover = false)
        {
            if (!failover)
            {
                await UpdateDistributedCacheAsync(document);
            }

            _memoryCache.Set(_options.CacheKey, document, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = _options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _options.SlidingExpiration
            });

            if (failover)
            {
                // Cache the id locally so that the memory cache is used during the 'FailoverRetryLatency'.
                _memoryCache.Set(_options.CacheIdKey, document.Identifier ?? "NULL", new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = _options.FailoverRetryLatency
                });

                return;
            }

            if (_isDistributed)
            {
                // Remove the id from the one second cache.
                _memoryCache.Remove(_options.CacheIdKey);
            }

            // Consistency: We may have been the last to update the cache but not with the last stored document.
            if (!_isVolatile && _options.CheckConsistency.Value)
            {
                (_, var stored) = await DocumentStore.GetOrCreateImmutableAsync<TDocument>();

                if (stored.Identifier != document.Identifier)
                {
                    if (_isDistributed)
                    {
                        await _distributedCache.RemoveAsync(_options.CacheIdKey);
                    }
                    else
                    {
                        _memoryCache.Remove(_options.CacheIdKey);
                    }
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
                data = await _options.Serializer.SerializeAsync(cached);
            }

            if (data == null)
            {
                return null;
            }

            return await _options.Serializer.DeserializeAsync<TDocument>(data);
        }

        private async Task UpdateDistributedCacheAsync(TDocument document)
        {
            if (_isDistributed)
            {
                var data = await _options.Serializer.SerializeAsync(document, _options.CompressThreshold);
                await _distributedCache.SetAsync(_options.CacheKey, data, _options);
                await _distributedCache.SetStringAsync(_options.CacheIdKey, document.Identifier ?? "NULL", _options);
            }
            else
            {
                _memoryCache.Set(_options.CacheIdKey, document.Identifier ?? "NULL");
            }
        }
    }
}
