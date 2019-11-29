using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Helper
{
    public static class TagExtensions
    {
        /// <summary>
        /// Tags are a less well known property of a taxonomy field
        /// managed by the tags editor and tags display mode. 
        /// </summary>
        public static string[] GetTags(this TaxonomyField taxonomyField)
        {
            return taxonomyField.Content.Tags.ToObject<string[]>();
        }
    }
}
