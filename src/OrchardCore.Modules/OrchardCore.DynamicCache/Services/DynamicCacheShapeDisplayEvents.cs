using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    /// <summary>
    /// Caches shapes in the default <see cref="IDynamicCacheService"/> implementation.
    /// It uses the shape's metadata cache context to define the cache parameters.
    /// </summary>
    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents, ITagRemovedEventHandler
    {
        private readonly IDynamicCacheService _dynamicCacheService;
        private readonly IDynamicCache _dynamicCache;

        public DynamicCacheShapeDisplayEvents(IDynamicCacheService dynamicCacheService, IDynamicCache dynamicCache)
        {
            _dynamicCacheService = dynamicCacheService;
            _dynamicCache = dynamicCache;
        }

        public async Task DisplayingAsync(ShapeDisplayContext context)
        {
            // The shape has cache settings and no content yet
            if (context.ShapeMetadata.IsCached && context.ChildContent == null)
            {
                var cacheContext = context.ShapeMetadata.Cache();
                var cachedContent = await _dynamicCacheService.GetCachedValueAsync(cacheContext);

                if (cachedContent != null)
                {
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
            else if (context.ChildContent != null)
            {
                using (var sw = new StringWriter())
                {
                    context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                    var content = sw.ToString();

                    content = await _dynamicCacheService.SetCachedValueAsync(cacheContext, content);

                    if (content != null)
                    {
                        context.ChildContent = new HtmlString(content);
                    }
                }
            }
        }

        public Task TagRemovedAsync(string tag, IEnumerable<string> keys)
        {
            return Task.WhenAll(keys.Select(key => _dynamicCache.RemoveAsync(key)));
        }
    }
}
