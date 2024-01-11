using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Pooling;
using OrchardCore.DynamicCache.Models;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    public class DefaultDynamicCacheService : IDynamicCacheService
    {
        public const string FailoverKey = "OrchardCore_DynamicCache_FailoverKey";
        public static readonly TimeSpan DefaultFailoverRetryLatency = TimeSpan.FromSeconds(30);

        private readonly PoolingJsonSerializer _serializer;
        private readonly ICacheContextManager _cacheContextManager;
        private readonly IDynamicCache _dynamicCache;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly DynamicCacheOptions _dynamicCacheOptions;
        private readonly CacheOptions _cacheOptions;
        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _localCache = new();
        private ITagCache _tagcache;

        public DefaultDynamicCacheService(
            ArrayPool<char> _arrayPool,
            ICacheContextManager cacheContextManager,
            IDynamicCache dynamicCache,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IOptions<DynamicCacheOptions> dynamicCacheOptions,
            IOptions<CacheOptions> options,
            ILogger<DefaultDynamicCacheService> logger)
        {
            _serializer = new PoolingJsonSerializer(_arrayPool);
            _cacheContextManager = cacheContextManager;
            _dynamicCache = dynamicCache;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
            _dynamicCacheOptions = dynamicCacheOptions.Value;
            _dynamicCacheOptions.FailoverRetryLatency ??= DefaultFailoverRetryLatency;
            _cacheOptions = options.Value;
            _logger = logger;
        }

        public async Task<string> GetCachedValueAsync(CacheContext context)
        {
            if (!_cacheOptions.Enabled)
            {
                return null;
            }

            var cacheKey = await GetCacheKey(context);

            context = await GetCachedContextAsync(cacheKey);
            if (context == null)
            {
                // We don't know the context, so we must treat this as a cache miss
                return null;
            }

            var content = await GetCachedStringAsync(cacheKey);

            return content;
        }

        public async Task SetCachedValueAsync(CacheContext context, string value)
        {
            if (!_cacheOptions.Enabled)
            {
                return;
            }

            var cacheKey = await GetCacheKey(context);

            _localCache[cacheKey] = value;
            var esi = _serializer.Serialize(CacheContextModel.FromCacheContext(context));

            await Task.WhenAll(
                SetCachedValueAsync(cacheKey, value, context),
                SetCachedValueAsync(GetCacheContextCacheKey(cacheKey), esi, context)
            );
        }

        public Task TagRemovedAsync(string tag, IEnumerable<string> keys)
        {
            return Task.WhenAll(keys.Select(key => _dynamicCache.RemoveAsync(key)));
        }

        private async Task SetCachedValueAsync(string cacheKey, string value, CacheContext context)
        {
            var failover = _memoryCache.Get<bool>(FailoverKey);
            if (failover)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = context.ExpiresOn,
                SlidingExpiration = context.ExpiresSliding,
                AbsoluteExpirationRelativeToNow = context.ExpiresAfter
            };

            // Default duration is sliding expiration (permanent as long as it's used)
            if (!options.AbsoluteExpiration.HasValue && !options.SlidingExpiration.HasValue && !options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                options.SlidingExpiration = new TimeSpan(0, 1, 0);
            }

            try
            {
                await _dynamicCache.SetAsync(cacheKey, bytes, options);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to write the '{CacheKey}' to the dynamic cache", cacheKey);

                _memoryCache.Set(FailoverKey, true, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = _dynamicCacheOptions.FailoverRetryLatency
                });

                return;
            }

            // Lazy load to prevent cyclic dependency
            _tagcache ??= _serviceProvider.GetRequiredService<ITagCache>();
            await _tagcache.TagAsync(cacheKey, context.Tags.ToArray());
        }

        private async Task<string> GetCacheKey(CacheContext context)
        {
            var cacheEntries = context.Contexts.Count > 0
                ? await _cacheContextManager.GetDiscriminatorsAsync(context.Contexts)
                : Enumerable.Empty<CacheContextEntry>();

            if (!cacheEntries.Any())
            {
                return context.CacheId;
            }

            var key = context.CacheId + "/" + cacheEntries.GetContextHash();
            return key;
        }

        private static string GetCacheContextCacheKey(string cacheKey)
        {
            return "cachecontext-" + cacheKey;
        }

        private async Task<string> GetCachedStringAsync(string cacheKey)
        {
            if (_localCache.TryGetValue(cacheKey, out var content))
            {
                return content;
            }

            var failover = _memoryCache.Get<bool>(FailoverKey);
            if (failover)
            {
                return null;
            }

            try
            {
                var bytes = await _dynamicCache.GetAsync(cacheKey);
                if (bytes == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read the '{CacheKey}' from the dynamic cache", cacheKey);

                _memoryCache.Set(FailoverKey, true, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = _dynamicCacheOptions.FailoverRetryLatency
                });
            }

            return null;
        }

        private async Task<CacheContext> GetCachedContextAsync(string cacheKey)
        {
            var cachedValue = await GetCachedStringAsync(GetCacheContextCacheKey(cacheKey));

            if (cachedValue == null)
            {
                return null;
            }

            var esiModel = _serializer.Deserialize<CacheContextModel>(cachedValue);
            return esiModel.ToCacheContext();
        }
    }
}
