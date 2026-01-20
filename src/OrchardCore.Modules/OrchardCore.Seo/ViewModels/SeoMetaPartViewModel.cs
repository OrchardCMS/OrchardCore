using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.ViewModels
{
    public class SeoMetaPartViewModel
    {
        public string PageTitle { get; set; }
        public bool Render { get; set; }

        public string MetaDescription { get; set; }
        public string Canonical { get; set; }

        public string MetaKeywords { get; set; }
        public string MetaRobots { get; set; }
        public string CustomMetaTags { get; set; }

        [BindNever]
        public SeoMetaPart SeoMetaPart { get; set; }

        [BindNever]
        public SeoMetaPartSettings Settings { get; set; }
    }
}
