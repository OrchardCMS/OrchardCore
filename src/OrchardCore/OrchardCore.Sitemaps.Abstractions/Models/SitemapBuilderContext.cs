using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapBuilderContext
    {
        public XDocument Result { get; set; }
        public IUrlHelper Url { get; set; }
    }
}
