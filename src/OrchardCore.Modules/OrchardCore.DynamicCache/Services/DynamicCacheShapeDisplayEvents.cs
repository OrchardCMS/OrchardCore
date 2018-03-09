//using System.Collections.Generic;
//using System.IO;
//using System.Text.Encodings.Web;
//using Microsoft.AspNetCore.Html;
//using OrchardCore.DisplayManagement.Implementation;
//using OrchardCore.Environment.Cache;

//namespace OrchardCore.DynamicCache.Services
//{
//    /// <summary>
//    /// Caches shapes in the default <see cref="IDynamicCacheService"/> implementation.
//    /// It uses the shape's metadata cache context to define the cache parameters.
//    /// </summary>
//    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents, ITagRemovedEventHandler
//    {
//        private readonly IDynamicCacheService _dynamicCacheService;
//        private readonly IDynamicCache _dynamicCache;

//        public DynamicCacheShapeDisplayEvents(IDynamicCacheService dynamicCacheService, IDynamicCache dynamicCache)
//        {
//            _dynamicCacheService = dynamicCacheService;
//            _dynamicCache = dynamicCache;
//        }

//        public void Displaying(ShapeDisplayContext context)
//        {
//            if (context.ShapeMetadata.IsCached && context.ChildContent == null)
//            {
//                var cacheContext = context.ShapeMetadata.Cache();
//                var cachedValue = _dynamicCacheService.GetCachedValueAsync(cacheContext).GetAwaiter().GetResult();// todo

//                if (cachedValue != null)
//                {
//                    context.ChildContent = new HtmlString(cachedValue);
//                }
//            }
//        }

//        public void Displayed(ShapeDisplayContext context)
//        {
//            var cacheContext = context.ShapeMetadata.Cache();

//            // If the shape is not cached, evaluate the ESIs
//            if(cacheContext == null)
//            {
//                if (context.ChildContent != null)
//                {
//                    string content;

//                    using (var sw = new StringWriter())
//                    {
//                        context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
//                        content = sw.ToString();
//                    }

//                    var esiResult = _dynamicCacheService.ProcessEdgeSideIncludesAsync(content).GetAwaiter().GetResult(); // todo
//                    context.ChildContent = new HtmlString(esiResult.Item2);
//                }
//                else
//                {
//                    context.ChildContent = HtmlString.Empty;
//                }
//            }
//            else if (context.ChildContent != null)
//            {
//                var childContent = _dynamicCacheService.SetCachedValueAsync(cacheContext, context.ChildContent.ToString()).GetAwaiter().GetResult(); //todo
//                if (childContent != null)
//                {
//                    context.ChildContent = new HtmlString(childContent);
//                }
//            }
//        }

//        public void TagRemoved(string tag, IEnumerable<string> keys)
//        {
//            foreach (var key in keys)
//            {
//                _dynamicCache.RemoveAsync(key);
//            }
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    /// <summary>
    /// Caches shapes in the default <see cref="IDistributedCache"/> implementation.
    /// It uses the shape's metadata cache context to define the cache parameters.
    /// </summary>
    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents, ITagRemovedEventHandler
    {
        private static char ContextSeparator = ';';

        private readonly ICacheContextManager _cacheContextManager;
        private readonly HashSet<CacheContext> _cached = new HashSet<CacheContext>();
        private readonly Dictionary<string, string> _localCache = new Dictionary<string, string>();
        private readonly IDynamicCache _dynamicCache;
        private readonly IServiceProvider _serviceProvider;

        public DynamicCacheShapeDisplayEvents(
            IDynamicCache dynamicCache,
            IServiceProvider serviceProvider,
            ICacheContextManager cacheContextManager)
        {
            _dynamicCache = dynamicCache;
            _serviceProvider = serviceProvider;
            _cacheContextManager = cacheContextManager;
        }

        public async Task DisplayingAsync(ShapeDisplayContext context)
        {
            // The shape has cache settings and no content yet
            if (context.ShapeMetadata.IsCached && context.ChildContent == null)
            {
                var cacheContext = context.ShapeMetadata.Cache();

                var cacheEntries = cacheContext.Contexts.Count > 0
                    ? await _cacheContextManager.GetContextAsync(cacheContext.Contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                var cacheKey = GetCacheKey(cacheContext.CacheId, cacheEntries);

                var content = await GetDistributedCacheAsync(cacheKey);

                if (content != null)
                {
                    content = await ProcessESIsAsync(content, GetDistributedCacheAsync);

                    if (content != null)
                    {
                        _cached.Add(cacheContext);
                        var contexts = String.Join(ContextSeparator.ToString(), cacheContext.Contexts.ToArray());
                        context.ChildContent = new HtmlString($"[[cache id='{cacheContext.CacheId}' contexts='{contexts}']]");
                    }
                }
            }
        }

        public async Task DisplayedAsync(ShapeDisplayContext context)
        {
            // TODO: Configure duration of sliding expiration

            var cacheContext = context.ShapeMetadata.Cache();

            // If the shape is not cached, evaluate the ESIs
            if (cacheContext == null)
            {
                if (context.ChildContent != null)
                {
                    string content;

                    using (var sw = new StringWriter())
                    {
                        context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                        content = sw.ToString();
                    }

                    content = await ProcessESIsAsync(content, GetDistributedCacheAsync);
                    context.ChildContent = new HtmlString(content);
                }
                else
                {
                    context.ChildContent = HtmlString.Empty;
                }
            }
            else if (!_cached.Contains(cacheContext) && context.ChildContent != null)
            {
                var cacheEntries = (await _cacheContextManager.GetContextAsync(cacheContext.Contexts));
                string cacheKey = GetCacheKey(cacheContext.CacheId, cacheEntries);

                using (var sw = new StringWriter())
                {
                    context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                    var content = sw.ToString();

                    _cached.Add(cacheContext);
                    _localCache[cacheKey] = content;
                    var contexts = String.Join(ContextSeparator.ToString(), cacheContext.Contexts.ToArray());
                    context.ChildContent = new HtmlString($"[[cache id='{cacheContext.CacheId}' contexts='{contexts}']]");

                    var bytes = Encoding.UTF8.GetBytes(content);

                    // Default duration is sliding expiration (permanent as long as it's used)
                    var options = new DistributedCacheEntryOptions
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

                    await _dynamicCache.SetAsync(cacheKey, bytes, options);

                    // Lazy load to prevent cyclic dependency
                    var tagCache = _serviceProvider.GetRequiredService<ITagCache>();
                    tagCache.Tag(cacheKey, cacheContext.Tags.ToArray());
                }
            }
        }

        private string GetCacheKey(string cacheId, IEnumerable<CacheContextEntry> cacheEntries)
        {
            if (cacheEntries.Count() == 0)
            {
                return cacheId;
            }

            var key = cacheId + "/" + cacheEntries.GetContextHash();
            return key;
        }

        /// <summary>
        /// Substitutes all ESIs with their actual content
        /// </summary>
        /// <returns>The updated content</returns>
        private async Task<string> ProcessESIsAsync(string content, Func<string, Task<string>> cache)
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
                    ? await _cacheContextManager.GetContextAsync(contexts)
                    : Enumerable.Empty<CacheContextEntry>();

                var cachedFragmentKey = GetCacheKey(id, cacheEntries);
                var htmlContent = await cache(cachedFragmentKey);

                // Expired child cache entry? Reprocess shape.
                if (htmlContent == null)
                {
                    return null;
                }

                htmlContent = await ProcessESIsAsync(htmlContent, cache);

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

        public Task TagRemovedAsync(string tag, IEnumerable<string> keys)
        {
            return Task.WhenAll(keys.Select(key => _dynamicCache.RemoveAsync(key)));
        }
    }
}