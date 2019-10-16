using Microsoft.AspNetCore.Mvc;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapBuilderContext
    {
        public string HostPrefix { get; set; }
        public IUrlHelper UrlHelper { get; set; }
        public ISitemapManager SitemapManager { get; set; }
    }
}
