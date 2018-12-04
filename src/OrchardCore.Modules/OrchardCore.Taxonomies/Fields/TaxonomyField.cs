using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Fields
{
    public class TaxonomyField : ContentField
    {
        public string TaxonomyContentItemId { get; set; }
        public string[] TermContentItemIds { get; set; } = Array.Empty<string>();
    }
}
