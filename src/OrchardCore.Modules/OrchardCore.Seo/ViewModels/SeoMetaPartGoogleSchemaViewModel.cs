using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.ViewModels
{
    public class SeoMetaPartGoogleSchemaViewModel
    {
        public string GoogleSchema { get; set; }

        [BindNever]
        public SeoMetaPart SeoMetaPart { get; set; }
    }
}
