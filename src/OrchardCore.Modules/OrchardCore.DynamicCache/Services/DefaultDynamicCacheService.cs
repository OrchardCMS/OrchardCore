using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrchardCore.DynamicCache.Models;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    public class DefaultDynamicCacheService : IDynamicCacheService
    {
        private readonly ICacheScopeManager _cacheScopeManager;
        private readonly ICacheContextManager _cacheContextManager;
        private readonly IDynamicCache _dynamicCache;
        private readonly IServiceProvider _serviceProvider;
        
        private readonly Dictionary<string, string> _localCache = new Dictionary<string, string>();

        public DefaultDynamicCacheService(ICacheScopeManager cacheScopeManager, ICacheContextManager cacheContextManager, IDynamicCache dynamicCache, IServiceProvider serviceProvider)
        {
            _cacheScopeManager = cacheScopeManager;
            _cacheContextManager = cacheContextManager;
            _dynamicCache = dynamicCache;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetCachedValueAsync(CacheContext context)
        {
            context = await GetCachedContextAsync(context);
            if (context == null)
            {
                // We don't know the context, so we must treat this as a cache miss
                return null;
            }
            
            var cacheKey = await GetCacheKey(context);

            var content = await GetCachedStringAsync(cacheKey);

            if (content == null)
            {
                return null;
            }

            // ProcessEdgeSideIncludesAsync will return null if one or more of the ESIs was a cache miss.
            // This allows the parent (and therefore the ESI) to be re-built and then rechached.
            return await ProcessEdgeSideIncludesAsync(content);
        }
        
        public async Task SetCachedValueAsync(CacheContext context, string value)
        {
            var cacheKey = await GetCacheKey(context);
            
            _localCache[cacheKey] = value;
            var esi = JsonConvert.SerializeObject(EdgeSideInclude.FromCacheContext(context));
            
            await Task.WhenAll(
                SetCachedValueAsync(cacheKey, value, context),
                SetCachedValueAsync(await GetCacheContextCacheKey(context), esi, context)
            );
        }

        private async Task SetCachedValueAsync(string cacheKey, string value, CacheContext context)
        {
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

            await _dynamicCache.SetAsync(cacheKey, bytes, options);

            // Lazy load to prevent cyclic dependency
            var tagCache = _serviceProvider.GetRequiredService<ITagCache>();
            tagCache.Tag(cacheKey, context.Tags.ToArray());
        }

        /// <summary>
        /// Substitutes all ESIs with their actual content
        /// </summary>
        /// <returns>The content string with all ESIs substituted out for their actualy content, or null if one or more of the ESIs is a cache miss</returns>
        public async Task<string> ProcessEdgeSideIncludesAsync(string content)
        {
            StringBuilder result = null;

            int lastIndex = 0, end = 0;
            var processed = false;

            while ((lastIndex = content.IndexOf("[[cache ", lastIndex)) > 0)
            {
                if (result == null)
                {
                    result = new StringBuilder(content.Length);
                }

                result.Append(content.Substring(end, lastIndex - end));

                processed = true;

                int startIndex;
                var esi = content.Substring(startIndex = content.IndexOf("esi='", lastIndex) + 5, (lastIndex = content.IndexOf("']]", startIndex)) - startIndex);

                end = content.IndexOf("]]", lastIndex) + 2;
                
                var esiModel = JsonConvert.DeserializeObject<EdgeSideInclude>(esi);
                var cacheContext = esiModel.ToCacheContext();

                _cacheScopeManager.EnterScope(cacheContext);

                try
                {
                    var htmlContent = await GetCachedValueAsync(cacheContext);

                    // Expired child cache entry? This and all parent cache items are invalid.
                    if (htmlContent == null)
                    {
                        return null;
                    }

                    htmlContent = await ProcessEdgeSideIncludesAsync(htmlContent);

                    if (htmlContent == null)
                    {
                        return null;
                    }

                    result.Append(htmlContent);
                }
                finally
                {
                    _cacheScopeManager.ExitScope();
                }
            }

            if (processed)
            {
                result.Append(content.Substring(end, content.Length - end));
                content = result.ToString();
            }

            return content;
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

        private async Task<string> GetCacheContextCacheKey(CacheContext context)
        {
            return "cachecontext-" + await GetCacheKey(context);
        }

        private async Task<string> GetCachedStringAsync(string cacheKey)
        {
            if (_localCache.TryGetValue(cacheKey, out var content))
            {
                return content;
            }

            var bytes = await _dynamicCache.GetAsync(cacheKey);
            if (bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private async Task<CacheContext> GetCachedContextAsync(CacheContext context)
        {
            var cachedValue = await GetCachedStringAsync(await GetCacheContextCacheKey(context));

            if (cachedValue == null)
            {
                return null;
            }

            var esiModel = JsonConvert.DeserializeObject<EdgeSideInclude>(cachedValue);
            return esiModel.ToCacheContext();
        }
    }
}