using Microsoft.AspNetCore.Mvc;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapBuilderContext
    {
        public string HostPrefix { get; set; }
        public IUrlHelper UrlHelper { get; set; }
        public ISitemapBuilder Builder { get; set; }
    }
}
