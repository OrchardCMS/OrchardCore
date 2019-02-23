using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OrchardCore.Sitemaps.Models
{
    [XmlRoot("sitemapindex", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class SitemapIndex
    {
        [XmlElement("sitemap")]
        public List<SitemapIndexItem> Items { get; set; } = new List<SitemapIndexItem>();
    }
}
