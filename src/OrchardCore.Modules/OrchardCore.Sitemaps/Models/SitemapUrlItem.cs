using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapUrlItem
    {
        [XmlElement("loc")]
        public string Location { get; set; }

        [XmlElement("lastmod")]
        public string LastModified { get; set; }

        public bool ShouldSerializeLastModified()
        {
            return !String.IsNullOrEmpty(LastModified);
        }

        [XmlElement("changefreq")]
        public string ChangeFrequency { get; set; }

        public bool ShouldSerializeChangeFrequency()
        {
            return !String.IsNullOrEmpty(ChangeFrequency);
        }

        [XmlElement("priority")]
        public float? Priority { get; set; }

        public bool ShouldSerializePriority()
        {
            return Priority.HasValue;
        }
    }
}
