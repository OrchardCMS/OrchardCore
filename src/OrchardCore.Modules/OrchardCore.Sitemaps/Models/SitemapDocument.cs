using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// The document of all Sitemaps stored on the system.
    /// </summary>
    public class SitemapDocument
    {
        public int Id { get; set; }
        public Dictionary<string, Sitemap> Sitemaps { get; set; } = new Dictionary<string, Sitemap>(StringComparer.OrdinalIgnoreCase);
    }
}
