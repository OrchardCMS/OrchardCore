using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapRouteDocument : Document
    {
        public Dictionary<string, string> SitemapIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> SitemapPaths { get; set; } = new Dictionary<string, string>();
    }
}
