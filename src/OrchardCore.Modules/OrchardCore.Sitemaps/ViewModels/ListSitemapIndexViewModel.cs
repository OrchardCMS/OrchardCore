using System.Collections.Generic;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class ListSitemapIndexViewModel
    {
        public IList<SitemapIndexListEntry> SitemapIndexes { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
        public dynamic Pager { get; set; }
    }

    public class SitemapIndexListEntry
    {
        public string SitemapId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
