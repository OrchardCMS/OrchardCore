using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapPartViewModel
    {
        public bool OverrideSitemapSetConfig { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public float Priority { get; set; }

        public bool Exclude { get; set; }

        [BindNever]
        public SitemapPart SitemapPart { get; set; }
        
        [BindNever]
        public SitemapPartSettings Settings { get; set; }

    }
}
