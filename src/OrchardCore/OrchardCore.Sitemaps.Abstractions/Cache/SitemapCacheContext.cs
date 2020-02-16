using System.Collections.Generic;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Cache
{
    public class SitemapCacheContext
    {
        public object CacheObject { get; set; }
        public IEnumerable<SitemapType> Sitemaps { get; set; }
    }
}
