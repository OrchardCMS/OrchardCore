using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.Services;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.EventHandlers
{
    /// <summary>
    /// Caches shapes in the default <see cref="IDynamicCacheService"/> implementation.
    /// It uses the shape's metadata cache context to define the cache parameters.
    /// </summary>
    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents, ITagRemovedEventHandler
    {
        private readonly HashSet<CacheContext> _cached = new HashSet<CacheContext>();

        private readonly IDynamicCacheService _dynamicCacheService;
        private readonly IDynamicCache _dynamicCache;
        private readonly ICacheScopeManager _cacheScopeManager;

        public DynamicCacheShapeDisplayEvents(IDynamicCacheService dynamicCacheService, IDynamicCache dynamicCache, ICacheScopeManager cacheScopeManager)
        {
            _dynamicCacheService = dynamicCacheService;
            _dynamicCache = dynamicCache;
            _cacheScopeManager = cacheScopeManager;
        }

        public async Task DisplayingAsync(ShapeDisplayContext context)
        {
            var debugMode = true;

            // The shape has cache settings and no content yet
            if (context.ShapeMetadata.IsCached && context.ChildContent == null)
            {
                if (debugMode)
                {
                    context.ShapeMetadata.Wrappers.Add("CachedShapeWrapper");
                }

                var cacheContext = context.ShapeMetadata.Cache();
                _cacheScopeManager.EnterScope(cacheContext);

                var cachedContent = await _dynamicCacheService.GetCachedValueAsync(cacheContext);

                if (cachedContent != null)
                {
                    // The contents of this shape was found in the cache.
                    // Add the cacheContext to _cached so that we don't try to cache the content again in the DisplayedAsync method.
                    _cached.Add(cacheContext);
                    context.ChildContent = new HtmlString(cachedContent);
                }
            }
        }

        public async Task DisplayedAsync(ShapeDisplayContext context)
        {
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

                    content = await _dynamicCacheService.ProcessEdgeSideIncludesAsync(content);
                    context.ChildContent = new HtmlString(content);
                }
                else
                {
                    context.ChildContent = HtmlString.Empty;
                }
            }
            else if (!_cached.Contains(cacheContext) && context.ChildContent != null)
            {
                // This shape should be cached, and the content did not come from the cache.
                // We need to insert the content of this shape into the cache, so that it can be retrieved on subsequent attempts.
                _cacheScopeManager.ExitScope(); // todo: how can we guarantee that this is called, even on failures?

                using (var sw = new StringWriter())
                {
                    context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                    await _dynamicCacheService.SetCachedValueAsync(cacheContext, sw.ToString());
                }
            }
        }

        // todo: should this method be in another service?
        public Task TagRemovedAsync(string tag, IEnumerable<string> keys)
        {
            return Task.WhenAll(keys.Select(key => _dynamicCache.RemoveAsync(key)));
        }
    }
}
