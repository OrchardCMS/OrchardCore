using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapPartViewModel
    {
        public bool OverrideSitemapConfig { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public int Priority { get; set; }

        public bool Exclude { get; set; }

        [BindNever]
        public SitemapPart SitemapPart { get; set; }
    }
}
