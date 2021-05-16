using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models
{
    // This part is added automatically to all terms
    public class TermPart : ContentPart
    {
        public string TaxonomyContentItemId { get; set; }
    }
}
