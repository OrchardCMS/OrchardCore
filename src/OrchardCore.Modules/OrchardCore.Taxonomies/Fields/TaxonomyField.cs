using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Fields;

public class TaxonomyField : ContentField
{
    [Description("The ID of the taxonomy content item.")]
    public string TaxonomyContentItemId { get; set; }

    [Description("The content item IDs of the selected taxonomy terms.")]
    public string[] TermContentItemIds { get; set; } = [];
}
