using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models
{
    // This part is added automatically to all terms
    public class TermPart : ContentPart
    {
        public string TaxonomyContentItemId { get; set; }
        // Items per page when ordering categorized content items for this term.
        public int OrderingPageSize { get; set; } = 0;
        // Set to true before querying categorized content items for this term, when ordering them. 
        public bool Ordering { get; set; } = false;
    }
}
