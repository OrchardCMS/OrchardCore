using System;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapBuilderContext
    {
        public IUrlHelper Url { get; set; }
        public ISitemapBuilder Builder { get; set; }
    }
}
