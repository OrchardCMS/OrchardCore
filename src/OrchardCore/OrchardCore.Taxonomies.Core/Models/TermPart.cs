using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models;

// This part is added automatically to all terms.
public class TermPart : ContentPart
{
    [Description("The content item identifier of the taxonomy containing this term.")]
    public string TaxonomyContentItemId { get; set; }
}
