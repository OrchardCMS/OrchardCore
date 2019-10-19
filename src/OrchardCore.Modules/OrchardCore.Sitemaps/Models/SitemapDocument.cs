using System;
using System.Collections.Immutable;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// The document of all Sitemaps stored on the system.
    /// </summary>
    public class SitemapDocument
    {
        public int Id { get; set; }
        public ImmutableDictionary<string, Sitemap> Sitemaps { get; set; } = ImmutableDictionary.Create<string, Sitemap>(StringComparer.OrdinalIgnoreCase);
    }
}
