using System;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Helper
{
    public static class TagElementExtensions
    {
        /// <summary>
        /// Tags are a less well known property of a taxonomy field
        /// managed by the tags editor and tags display mode. 
        /// </summary>
        public static string[] GetTags(this TaxonomyField taxonomyField)
        {
            var tagsElement = taxonomyField.Get<TagElement>(nameof(TagElement));

            return tagsElement != null ? tagsElement.Tags : Array.Empty<string>();
        }
    }
}
