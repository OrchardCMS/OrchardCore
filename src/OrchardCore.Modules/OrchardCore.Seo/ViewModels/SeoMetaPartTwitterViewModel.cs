using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.ViewModels
{
    public class SeoMetaPartTwitterViewModel
    {
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterCard { get; set; }
        public string TwitterCreator { get; set; }
        public string TwitterSite { get; set; }

        [BindNever]
        public SeoMetaPart SeoMetaPart { get; set; }
    }
}
