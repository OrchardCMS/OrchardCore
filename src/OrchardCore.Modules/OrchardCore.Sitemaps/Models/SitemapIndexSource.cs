using System;

namespace OrchardCore.Sitemaps.Models
{
    // This is a single use Sitemap Index Source.
    // It is a SitemapSource because it works better when handling routing lookups.
    // but it intentionally is hidden from normal sources and, 
    // will only ever be used a single time inside a sitemap.
    // For that reason there are no drivers.

    public class SitemapIndexSource : SitemapSource
    {
        public string[] ContainedSitemapIds { get; set; } = Array.Empty<string>();
    }
}
