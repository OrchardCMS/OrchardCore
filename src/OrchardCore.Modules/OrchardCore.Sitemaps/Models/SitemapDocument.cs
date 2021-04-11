using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapDocument : Document
    {
        public IDictionary<string, SitemapType> Sitemaps { get; set; } = new Dictionary<string, SitemapType>();
        public Dictionary<string, string> SitemapIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> SitemapPaths { get; set; } = new Dictionary<string, string>();
        public bool IsBuilt { get; set; }
    }
}
