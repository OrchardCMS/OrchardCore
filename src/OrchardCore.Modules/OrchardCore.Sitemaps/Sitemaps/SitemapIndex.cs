using System;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Sitemaps
{
    public class SitemapIndex : Sitemap
    {
        public string[] ContainedSitemapIds { get; set; } = Array.Empty<string>();
        public override bool IsContainable => false;
    }
}
