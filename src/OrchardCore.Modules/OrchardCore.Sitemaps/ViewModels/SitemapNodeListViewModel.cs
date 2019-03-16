using System.Collections.Generic;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapNodeListViewModel
    {
        public Models.SitemapSet SitemapSet { get; set; }
        //probably don't need this
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
