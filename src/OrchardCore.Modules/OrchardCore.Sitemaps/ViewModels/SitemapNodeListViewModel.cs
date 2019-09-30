using System.Collections.Generic;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapNodeListViewModel
    {
        public SitemapSet SitemapSet { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
