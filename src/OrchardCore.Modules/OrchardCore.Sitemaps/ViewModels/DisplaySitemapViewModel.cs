using System.Collections.Generic;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class DisplaySitemapViewModel
    {
        public SitemapType Sitemap { get; set; }
        public IEnumerable<dynamic> Items { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
