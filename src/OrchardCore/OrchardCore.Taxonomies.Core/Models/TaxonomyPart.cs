using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models;

// This part is added automatically to all taxonomies.
public class TaxonomyPart : ContentPart
{
    [Description("The content type name used for terms in this taxonomy.")]
    public string TermContentType { get; set; }

    [Description("The hierarchical term content items in this taxonomy.")]
    public List<ContentItem> Terms { get; set; } = [];
}
