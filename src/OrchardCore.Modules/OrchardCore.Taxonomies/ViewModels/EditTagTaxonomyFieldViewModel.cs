using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.ViewModels;

public class EditTagTaxonomyFieldViewModel
{
    public string TermContentItemIds { get; set; }

    /// <summary>
    /// Gets or sets the terms of the taxonomy, serialized as a JSON array of <see cref="TagTermEntry"/> objects.
    /// </summary>
    [BindNever]
    public string TagTermEntries { get; set; }

    /// <summary>
    /// Gets or sets the terms of the taxonomy, the selected ones first in the order of the field.
    /// </summary>
    [BindNever]
    public IList<TagTermEntry> TagTerms { get; set; } = [];

    [BindNever]
    public ContentItem Taxonomy { get; set; }

    [BindNever]
    public TaxonomyField Field { get; set; }

    [BindNever]
    public ContentPart Part { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}

public class TagTermEntry
{
    public bool Selected { get; set; }
    public string ContentItemId { get; set; }
    public string DisplayText { get; set; }
    public bool IsLeaf { get; set; }
}
