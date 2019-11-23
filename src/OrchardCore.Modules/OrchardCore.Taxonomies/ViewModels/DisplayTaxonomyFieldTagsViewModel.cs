using OrchardCore.Taxonomies.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldTagsViewModel : DisplayTaxonomyFieldViewModel
    {
        public string[] TagTermDisplayTexts => Field.TagTermDisplayTexts;
    }
}
