using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Fields
{
    public class TaxonomyField : ContentField
    {
        public string TaxonomyContentItemId { get; set; }
        public string[] TermContentItemIds { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Only populated when using the tag editor.
        /// Requires resaving the content item if updating the taxonomy tag term display text.
        /// </summary>
        public string[] TagTermDisplayTexts { get; set; } = Array.Empty<string>();
    }
}
