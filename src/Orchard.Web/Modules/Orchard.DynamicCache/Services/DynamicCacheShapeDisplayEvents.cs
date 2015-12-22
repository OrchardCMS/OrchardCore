using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.WebEncoders;
using Orchard.DisplayManagement.Implementation;
using System;
using System.IO;
using System.Text;

namespace Orchard.DynamicCache.Services
{
    public class DynamicCacheShapeDisplayEvents : IShapeDisplayEvents
    {
        private readonly IDistributedCache _distributedMemoryCache;
        private bool _isCached;

        public DynamicCacheShapeDisplayEvents(IDistributedCache distributedMemoryCache)
        {
            _distributedMemoryCache = distributedMemoryCache;
        }

        public void Displaying(ShapeDisplayingContext context)
        {
            string cacheId = context.ShapeMetadata.CacheContext.CacheId;
            if (!String.IsNullOrEmpty(cacheId))
            {
                var bytes = _distributedMemoryCache.Get(cacheId);
                if(bytes != null)
                {
                    _isCached = true;
                    var content = Encoding.UTF8.GetString(bytes);
                    context.ChildContent = new HtmlString(content);
                }
            }
        }

        public void Displayed(ShapeDisplayedContext context)
        {
            if (!_isCached && context.ChildContent != null)
            {
                string cacheId = context.ShapeMetadata.CacheContext.CacheId;
                if (!String.IsNullOrEmpty(cacheId))
                {
                    using (var sw = new StringWriter())
                    {
                        context.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                        var content = sw.ToString();
                        var bytes = Encoding.UTF8.GetBytes(content);
                        _distributedMemoryCache.Set(cacheId, bytes);
                    }
                }
            }
        }
    }
}
