using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models;

// This part is added automatically to all taxonomies
public class TaxonomyPart : ContentPart
{
    public string TermContentType { get; set; }
    public List<ContentItem> Terms { get; set; } = [];
}
