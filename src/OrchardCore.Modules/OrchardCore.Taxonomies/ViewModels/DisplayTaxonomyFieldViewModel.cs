using OrchardCore.Taxonomies.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldViewModel
    {
        public TaxonomyField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
