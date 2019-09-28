using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapNodeEditViewModel
    {
        public string SitemapSetId { get; set; }
        public string SitemapNodeId { get; set; }
        public string SitemapNodeType { get; set; }
        public dynamic Editor { get; set; }

        [BindNever]
        public SitemapNode SitemapNode { get; set; }

    }
}
