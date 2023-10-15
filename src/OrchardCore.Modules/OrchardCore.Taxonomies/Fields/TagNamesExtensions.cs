using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Taxonomies.Fields
{
    public static class TagNamesExtensions
    {
        /// <summary>
        /// Tag names are a less well known property of a taxonomy field
        /// managed by the tags editor and tags display mode.
        /// </summary>
        public static string[] GetTagNames(this TaxonomyField taxonomyField) =>
            taxonomyField.GetArrayProperty<string>("TagNames");

        /// <summary>
        /// Tags names are a less well known property of a taxonomy field
        /// managed by the tags editor and tags display mode.
        /// They are updated only when saving the content item.
        /// </summary>
        public static void SetTagNames(this TaxonomyField taxonomyField, string[] tagNames) =>
            taxonomyField.SetProperty("TagNames", tagNames);
    }
}
