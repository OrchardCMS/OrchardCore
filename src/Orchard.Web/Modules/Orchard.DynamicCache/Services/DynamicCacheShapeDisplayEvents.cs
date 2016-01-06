using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.WebEncoders;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.DynamicCache.Services
{
    /// <summary>
    /// Caches shapes in the default <see cref="IDistributedCache"/> implementation.
    /// It uses the shape's metadata cache context to define the cache parameters.
    /// </summary>
    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents, ITagRemovedEventHandler
    {
        private static char ContextSeparator = ';';

        private readonly ICacheContextManager _cacheContextManager;
        private readonly HashSet<ShapeMetadataCacheContext> _cached = new HashSet<ShapeMetadataCacheContext>();
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly IDynamicCache _dynamicCache;
        private readonly ITagCache _tagCache;

        public DynamicCacheShapeDisplayEvents(
            IDynamicCache dynamicCache,
            ITagCache tagCache,
            ICacheContextManager cacheContextManager)
        {
            _dynamicCache = dynamicCache;
            _tagCache = tagCache;
            _cacheContextManager = cacheContextManager;
        }

        public void Displaying(ShapeDisplayingContext context)
        {
            if (context.ShapeMetadata.IsCached && context.ChildContent == null)
            {
                var cacheContext = context.ShapeMetadata.Cache();
                var cacheEntries = GetCacheEntries(cacheContext).ToList();
                string cacheKey = GetCacheKey(cacheContext.CacheId, cacheEntries);

                var content = GetDistributedCache(cacheKey);
                if (content != null)
                {
                    if (ProcessESIs(ref content, GetDistributedCache))
                    {
                        _cached.Add(cacheContext);
                        var contexts = String.Join(ContextSeparator.ToString(), cacheContext.Contexts.ToArray());
                        context.ChildContent = new HtmlString($"[[cache id='{cacheContext.CacheId}' contexts='{contexts}']]");
                    }
                }
            }
        }

        public void Displayed(ShapeDisplayedContext context)
        {
            // TODO: Configure duration of sliding expiration

            var cacheContext = context.ShapeMetadata.Cache();

            // If the shape is not cached, evaluate the ESIs
            if(cacheContext == null)
            {
                string content;
                using (var sw = new StringWriter())
                {
                    context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                    content = sw.ToString();
                }

                ProcessESIs(ref content, GetDistributedCache);
                context.ChildContent = new HtmlString(content);
            }
            else if (!_cached.Contains(cacheContext) && context.ChildContent != null)
            {
                var cacheEntries = GetCacheEntries(cacheContext).ToList();
                string cacheKey = GetCacheKey(cacheContext.CacheId, cacheEntries);

                using (var sw = new StringWriter())
                {
                    context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                    var content = sw.ToString();

                    _cached.Add(cacheContext);
                    _cache[cacheKey] = content;
                    var contexts = String.Join(ContextSeparator.ToString(), cacheContext.Contexts.ToArray());
                    context.ChildContent = new HtmlString($"[[cache id='{cacheContext.CacheId}' contexts='{contexts}']]");

                    var bytes = Encoding.UTF8.GetBytes(content);

                    // Default duration is sliding expiration (permanent as long as it's used)
                    DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = new TimeSpan(0, 1, 0)
                    };

                    // If a custom duration is specified, replace the default options
                    if (cacheContext.Duration.HasValue)
                    {
                        options = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = cacheContext.Duration
                        };
                    }

                    _dynamicCache.SetAsync(cacheKey, bytes, options).Wait();
                    _tagCache.Tag(cacheKey, cacheContext.Tags.ToArray());
                }
            }

        }

        private string GetCacheKey(string cacheId, IEnumerable<CacheContextEntry> cacheEntries)
        {
            var key = cacheId + "/" + cacheEntries.GetContextHash();
            return key;
        }

        private IEnumerable<CacheContextEntry> GetCacheEntries(ShapeMetadataCacheContext cacheContext)
        {
            // All contexts' entries
            foreach(var entry in GetCacheEntries(cacheContext.Contexts))
            {
                yield return entry;
            }
        }

        private IEnumerable<CacheContextEntry> GetCacheEntries(IEnumerable<string> contexts)
        {
            return _cacheContextManager.GetContext(contexts);
        }

        private bool ProcessESIs(ref string content, Func<string, string> cache)
        {
            var result = new StringBuilder(content.Length);

            int lastIndex = 0, startIndex = 0, start = 0, end = 0;
            bool processed = false;
            while ((lastIndex = content.IndexOf("[[cache ", lastIndex)) > 0)
            {
                result.Append(content.Substring(end, lastIndex - end));

                processed = true;
                start = lastIndex;

                var id = content.Substring(startIndex = content.IndexOf("id='", lastIndex) + 4, (lastIndex = content.IndexOf("'", startIndex)) - startIndex);
                var contexts = content.Substring(startIndex = content.IndexOf("contexts='", lastIndex) + 10, (lastIndex = content.IndexOf("'", startIndex)) - startIndex).Split(ContextSeparator);

                end = content.IndexOf("]]", lastIndex) + 2;

                var cacheEntries = GetCacheEntries(contexts);
                var cachedFragmentKey = GetCacheKey(id, cacheEntries);
                var htmlContent = cache(cachedFragmentKey);

                // Expired child cache entry? Reprocess shape.
                if(htmlContent == null)
                {
                    return false;
                }

                if(!ProcessESIs(ref htmlContent, cache))
                {
                    return false;
                }

                result.Append(htmlContent);
            }

            if (processed)
            {
                result.Append(content.Substring(end, content.Length - end));
                content = result.ToString();
            }
            
            return true;
        }

        private string GetDistributedCache(string cacheKey)
        {
            string content;
            if(_cache.TryGetValue(cacheKey, out content))
            {
                return content;
            }

            var bytes = _dynamicCache.GetAsync(cacheKey).Result;
            if (bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public void TagRemoved(string tag, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                _dynamicCache.RemoveAsync(key);
            }
        }
    }
}
