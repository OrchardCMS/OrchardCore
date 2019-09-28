using System.Collections.Generic;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapSetListViewModel
    {
        public IList<SitemapSetEntry> SitemapSet { get; set; }
        public SitemapSetListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SitemapSetEntry
    {
        public SitemapSet SitemapSet { get; set; }
        public bool IsChecked { get; set; }
    }
}
