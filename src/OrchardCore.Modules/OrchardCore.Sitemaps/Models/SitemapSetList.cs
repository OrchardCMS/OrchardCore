using System;
using System.Collections.Generic;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// The list of all the SitemapSets stored on the system.
    /// </summary>
    public class SitemapSetList
    {
        public int Id { get; set; }
        public List<SitemapSet> SitemapSet { get; set; } = new List<SitemapSet>();
    }
}
