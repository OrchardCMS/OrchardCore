using System.Collections.Generic;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class ListSitemapIndexViewModel
    {
        public IList<SitemapIndexListEntry> SitemapIndexes { get; set; }
        public SitemapIndexListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SitemapIndexListEntry
    {
        public string SitemapId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class SitemapIndexListOptions
    {
        public string Search { get; set; }
    }

}
