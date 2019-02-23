using System;
using System.Collections.Generic;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapEntry
    {
        public SitemapEntry()
        {
        }

        //public string ProviderName { get; private set; }
        //public string ProviderDisplayName { get; private set; }
        public string Url { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        public ChangeFrequency? ChangeFrequency { get; set; }
        public float? Priority { get; set; }
        //public string Context { get; set; }
        //public IList<ImageEntry> Images { get; set; }
    }
}