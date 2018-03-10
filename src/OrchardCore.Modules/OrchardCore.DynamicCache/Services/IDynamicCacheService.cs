using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    // todo: is this the correct name?
    public interface IDynamicCacheService
    {
        Task<string> GetCachedValueAsync(CacheContext context);
        Task<string> SetCachedValueAsync(CacheContext context, string value);
        Task<string> ProcessEdgeSideIncludesAsync(string content);
    }

    public class DynamicCacheService : IDynamicCacheService
    {
        private static char ContextSeparator = ';';

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

        public async Task<string> GetCachedValueAsync(CacheContext context)
        {
            _cacheScopeManager.EnterScope(context);

            try
            {
                var cacheEntries = context.Contexts.Count > 0
                    ? await _cacheContextManager.GetDiscriminatorsAsync(context.Contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                var cacheKey = GetCacheKey(context.CacheId, cacheEntries);

                var content = await GetDistributedCacheAsync(cacheKey);

                if (content != null)
                {
                    content = await ProcessEdgeSideIncludesAsync(content);

                    if (content != null)
                    {
                        _cached.Add(context);
                        var contexts = String.Join(ContextSeparator.ToString(), context.Contexts.ToArray());
                        return $"[[cache id='{context.CacheId}' contexts='{contexts}']]";
                    }
                }
            }
            finally
            {
                _cacheScopeManager.ExitScope();
            }

            return null;
        }

        public async Task<string> SetCachedValueAsync(CacheContext context, string value)
        {
            if (_cached.Contains(context))
            {
                return null;
            }

            var cacheEntries = await _cacheContextManager.GetDiscriminatorsAsync(context.Contexts);
            string cacheKey = GetCacheKey(context.CacheId, cacheEntries);
            
            
            _cached.Add(context);
            _localCache[cacheKey] = value;
            var contexts = String.Join(ContextSeparator.ToString(), context.Contexts.ToArray());
            var result = $"[[cache id='{context.CacheId}' contexts='{contexts}']]";

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

            return result;
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

                var id = content.Substring(startIndex = content.IndexOf("id='", lastIndex) + 4, (lastIndex = content.IndexOf("'", startIndex)) - startIndex);
                var contexts = content.Substring(startIndex = content.IndexOf("contexts='", lastIndex) + 10, (lastIndex = content.IndexOf("'", startIndex)) - startIndex).Split(ContextSeparator);

                end = content.IndexOf("]]", lastIndex) + 2;

                var cacheEntries = contexts.Length > 0
                    ? await _cacheContextManager.GetDiscriminatorsAsync(contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                var cachedFragmentKey = GetCacheKey(id, cacheEntries);
                var htmlContent = await GetDistributedCacheAsync(cachedFragmentKey);

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

        private async Task<string> GetDistributedCacheAsync(string cacheKey)
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
    }
}