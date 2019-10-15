using System.Collections.Generic;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapListViewModel
    {

        public IList<dynamic> Sitemaps { get; set; }
        public SitemapListOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }

    public class SitemapListEntry
    {
        public dynamic Shape { get; set; }
    }
}
