using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapBuilderContext
    {
        public string HostPrefix { get; set; }
        public IUrlHelper UrlHelper { get; set; }
        public SitemapResponse Response { get; set; }
    }

    public class SitemapResponse
    {
        public XElement ResponseElement { get; set; }
    }
}
