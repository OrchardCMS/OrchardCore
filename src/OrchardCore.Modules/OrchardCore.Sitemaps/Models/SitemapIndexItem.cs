using System;
using System.Xml.Serialization;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapIndexItem
    {
        [XmlElement("loc")]
        public string Location { get; set; }

        [XmlElement("lastmod")]
        public string LastModifiedUtc { get; set; }
    }
}