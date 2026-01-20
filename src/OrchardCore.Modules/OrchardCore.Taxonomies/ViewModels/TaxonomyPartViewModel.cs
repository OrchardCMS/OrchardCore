using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class TaxonomyPartViewModel
    {
        public string TaxonomyContentItemId => ContentItem.ContentItemId;

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public TaxonomyPart TaxonomyPart { get; set; }
    }
}
