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
    public interface IDynamicCacheService
    {
        Task<string> GetCachedValueAsync(CacheContext context);
        Task<string> SetCachedValueAsync(CacheContext context, string value);
        Task<Tuple<bool, string>> ProcessEdgeSideIncludesAsync(string content); //todo- tuple?!
    }

    public class DynamicCacheService : IDynamicCacheService
    {
        private static char ContextSeparator = ';';

        private readonly ICacheScopeManager _cacheScopeManager;
        private readonly ICacheContextManager _cacheContextManager;
        private readonly IDynamicCache _dynamicCache;
        private readonly IServiceProvider _serviceProvider;

        private readonly HashSet<CacheContext> _cached = new HashSet<CacheContext>();
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public DynamicCacheService(ICacheScopeManager cacheScopeManager, ICacheContextManager cacheContextManager, IDynamicCache dynamicCache, IServiceProvider serviceProvider)
        {
            _cacheScopeManager = cacheScopeManager;
            _cacheContextManager = cacheContextManager;
            _dynamicCache = dynamicCache;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetCachedValueAsync(CacheContext context)
        {
            var cacheEntries = (await GetCacheEntriesAsync(context)).ToList();
            var cacheKey = GetCacheKey(context.CacheId, cacheEntries);

            var value = await GetDistributedCacheAsync(cacheKey);
            if (value == null)
            {
                return null;
            }

            _cacheScopeManager.EnterScope(context);

            try
            {
                var esiResult = await ProcessEdgeSideIncludesAsync(value);
                if (esiResult.Item1)
                {
                    _cached.Add(context);
                    var contexts = String.Join(ContextSeparator.ToString(), context.Contexts.ToArray());
                    return $"[[cache id='{context.CacheId}' contexts='{contexts}']]";
                }
            }
            finally
            {
                _cacheScopeManager.ExitScope();
            }

            return value;
        }

        public async Task<string> SetCachedValueAsync(CacheContext context, string value)
        {
            if (_cached.Contains(context))
            {
                // We've already cached this result, so no need to do it again
                return null;
            }

            var cacheEntries = (await GetCacheEntriesAsync(context)).ToList();
            var cacheKey = GetCacheKey(context.CacheId, cacheEntries);
            
            _cached.Add(context);
            _cache[cacheKey] = value;
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

            _dynamicCache.SetAsync(cacheKey, bytes, options).Wait();

            // Lazy load to prevent cyclic dependency
            var tagCache = _serviceProvider.GetRequiredService<ITagCache>();
            tagCache.Tag(cacheKey, context.Tags.ToArray());

            return result;
        }

        private string GetCacheKey(string cacheId, IEnumerable<CacheContextEntry> cacheEntries)
        {
            var key = cacheId + "/" + cacheEntries.GetContextHash();
            return key;
        }

        private async Task<IEnumerable<CacheContextEntry>> GetCacheEntriesAsync(CacheContext cacheContext)
        {
            // All contexts' entries
            return await GetCacheEntriesAsync(cacheContext.Contexts);
        }

        private async Task<IEnumerable<CacheContextEntry>> GetCacheEntriesAsync(IEnumerable<string> contexts)
        {
            return await _cacheContextManager.GetDiscriminatorsAsync(contexts);
        }

        public async Task<Tuple<bool, string>> ProcessEdgeSideIncludesAsync(string content)
        {
            var result = new StringBuilder(content.Length);

            int lastIndex = 0, startIndex = 0, start = 0, end = 0;
            var processed = false;
            while ((lastIndex = content.IndexOf("[[cache ", lastIndex)) > 0)
            {
                result.Append(content.Substring(end, lastIndex - end));

                processed = true;
                start = lastIndex;

                var id = content.Substring(startIndex = content.IndexOf("id='", lastIndex) + 4, (lastIndex = content.IndexOf("'", startIndex)) - startIndex);
                var contexts = content.Substring(startIndex = content.IndexOf("contexts='", lastIndex) + 10, (lastIndex = content.IndexOf("'", startIndex)) - startIndex).Split(ContextSeparator);

                end = content.IndexOf("]]", lastIndex) + 2;

                var cacheEntries = await GetCacheEntriesAsync(contexts);
                var cachedFragmentKey = GetCacheKey(id, cacheEntries);
                var htmlContent = await GetDistributedCacheAsync(cachedFragmentKey);

                // Expired child cache entry? Reprocess shape.
                if (htmlContent == null)
                {
                    return new Tuple<bool, string>(false, content);
                }

                var esiResult = await ProcessEdgeSideIncludesAsync(htmlContent);
                if (!esiResult.Item1)
                {
                    return new Tuple<bool, string>(false, content);
                }

                result.Append(htmlContent);
            }

            if (processed)
            {
                result.Append(content.Substring(end, content.Length - end));
                content = result.ToString();
            }

            return new Tuple<bool, string>(true, content);
        }

        private async Task<string> GetDistributedCacheAsync(string cacheKey)
        {
            string content;
            if (_cache.TryGetValue(cacheKey, out content))
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