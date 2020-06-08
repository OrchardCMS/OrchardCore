using System.Collections.Generic;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class ListSitemapViewModel
    {
        public IList<SitemapListEntry> Sitemaps { get; set; }
        public SitemapListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SitemapListEntry
    {
        public string SitemapId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class SitemapListOptions
    {
        public string Search { get; set; }
    }

}
