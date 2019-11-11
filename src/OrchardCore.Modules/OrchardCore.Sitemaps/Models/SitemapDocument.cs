using System.Collections.Generic;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapDocument
    {
        public IDictionary<string, SitemapType> Sitemaps { get; set; } = new Dictionary<string, SitemapType>();
    }
}
