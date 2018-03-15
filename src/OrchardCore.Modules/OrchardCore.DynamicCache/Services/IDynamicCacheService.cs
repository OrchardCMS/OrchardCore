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
    // todo: is this the correct name?
    public interface IDynamicCacheService
    {
        // Takes the cache id, checks for cached contexts based on id, returns null if miss or cached content if hit
        Task<string> GetCachedValueAsync(string cacheId);
        // I don't think this method is needed
        //Task<string> GetCachedValueAsync(CacheContext context);
        // needs to also cache the context by the cache id
        Task<string> SetCachedValueAsync(CacheContext context, Func<Task<string>> contentFactory);
        Task<string> ProcessEdgeSideIncludesAsync(string value);
    }

    public class DynamicCacheService : IDynamicCacheService
    {
        //private static char ContextSeparator = ';';

        private readonly ICacheScopeManager _cacheScopeManager;
        private readonly ICacheContextManager _cacheContextManager;
        private readonly IDynamicCache _dynamicCache;
        private readonly IServiceProvider _serviceProvider;

        private readonly HashSet<CacheContext> _cached = new HashSet<CacheContext>();
        private readonly Dictionary<string, string> _localCache = new Dictionary<string, string>();

        public DynamicCacheService(ICacheScopeManager cacheScopeManager, ICacheContextManager cacheContextManager, IDynamicCache dynamicCache, IServiceProvider serviceProvider)
        {
            _cacheScopeManager = cacheScopeManager;
            _cacheContextManager = cacheContextManager;
            _dynamicCache = dynamicCache;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetCachedValueAsync(string cacheId)
        {
            //_cacheScopeManager.EnterScope(context);

            try
            {
                var context = await GetCachedContextAsync(cacheId);
                if (context == null)
                {
                    // We don't know the context, so we must treat this as a cache miss
                    return null;
                }

                var cacheEntries = context.Contexts.Count > 0
                    ? await _cacheContextManager.GetDiscriminatorsAsync(context.Contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                var cacheKey = GetCacheKey(context.CacheId, cacheEntries);

                var content = await GetCachedStringAsync(cacheKey);

                if (content != null)
                {
                    content = await ProcessEdgeSideIncludesAsync(content);

                    if (content != null)
                    {
                        _cached.Add(context);
                        return content;
                        //return $"[[cache esi='{JsonConvert.SerializeObject(EdgeSideInclude.FromCacheContext(context))}']]";
                    }
                }
            }
            finally
            {
                //_cacheScopeManager.ExitScope();
            }

            return null;
        }
        
        public async Task<string> SetCachedValueAsync(CacheContext context, Func<Task<string>> contentFactory)
        {
            //_cacheScopeManager.EnterScope(context);
            if (_cached.Contains(context))
            {
                return null;
            }

            var cacheEntries = await _cacheContextManager.GetDiscriminatorsAsync(context.Contexts);
            string cacheKey = GetCacheKey(context.CacheId, cacheEntries);

            var value = await contentFactory();

            _cached.Add(context);
            _localCache[cacheKey] = value;
            var esi = JsonConvert.SerializeObject(EdgeSideInclude.FromCacheContext(context));
            var result = $"[[cache esi='{esi}']]";

            // todo: when all
            await SetCachedValueAsync(cacheKey, value, context);
            await SetCachedValueAsync("cachecontext-" + context.CacheId, esi, context);

            //_cacheScopeManager.ExitScope();
            return result;
        }

        private async Task SetCachedValueAsync(string cacheKey, string value, CacheContext context)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = context.SlidingExpirationWindow,
                AbsoluteExpirationRelativeToNow = context.Duration
            };

            // Default duration is sliding expiration (permanent as long as it's used)
            if (!options.SlidingExpiration.HasValue && !options.AbsoluteExpirationRelativeToNow.HasValue)
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
        /// <returns>The updated content</returns>
        public async Task<string> ProcessEdgeSideIncludesAsync(string content)
        {
            StringBuilder result = null;

            int lastIndex = 0, startIndex = 0, start = 0, end = 0;
            var processed = false;
            while ((lastIndex = content.IndexOf("[[cache ", lastIndex)) > 0)
            {
                if (result == null)
                {
                    result = new StringBuilder(content.Length);
                }

                result.Append(content.Substring(end, lastIndex - end));

                processed = true;
                start = lastIndex;

                var esi = content.Substring(startIndex = content.IndexOf("esi='", lastIndex) + 5, (lastIndex = content.IndexOf("']]", startIndex)) - startIndex);
                //var id = content.Substring(startIndex = content.IndexOf("id='", lastIndex) + 4, (lastIndex = content.IndexOf("'", startIndex)) - startIndex);
                //var contexts = content.Substring(startIndex = content.IndexOf("contexts='", lastIndex) + 10, (lastIndex = content.IndexOf("'", startIndex)) - startIndex).Split(ContextSeparator);
                //var tags = content.Substring(startIndex = content.IndexOf("tags='", lastIndex) + 6, (lastIndex = content.IndexOf("'", startIndex)) - startIndex).Split(ContextSeparator);

                end = content.IndexOf("]]", lastIndex) + 2;
                
                var esiModel = JsonConvert.DeserializeObject<EdgeSideInclude>(esi);
                var cacheContext = esiModel.ToCacheContext();

                var cacheEntries = cacheContext.Contexts.Count > 0
                    ? await _cacheContextManager.GetDiscriminatorsAsync(cacheContext.Contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                _cacheScopeManager.EnterScope(cacheContext);

                try
                {
                    var cachedFragmentKey = GetCacheKey(cacheContext.CacheId, cacheEntries);
                    var htmlContent = await GetCachedValueAsync(cachedFragmentKey);

                    // Expired child cache entry? Reprocess shape.
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

        private string GetCacheKey(string cacheId, IEnumerable<CacheContextEntry> cacheEntries)
        {
            if (!cacheEntries.Any())
            {
                return cacheId;
            }

            var key = cacheId + "/" + cacheEntries.GetContextHash();
            return key;
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

        private async Task<CacheContext> GetCachedContextAsync(string cacheId)
        {
            var cachedValue = await GetCachedStringAsync("cachecontext-" + cacheId);

            if (cachedValue == null)
            {
                return null;
            }

            var esiModel = JsonConvert.DeserializeObject<EdgeSideInclude>(cachedValue);
            return esiModel.ToCacheContext();
        }
    }
}