using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Taxonomies.Models
{
    /// <summary>
    /// The tag element is applied to a taxonomy field when the tags display mode is used.
    /// </summary>
    public class TagElement : ContentElement
    {
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}
