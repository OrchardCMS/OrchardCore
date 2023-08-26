using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public static class ContentMetadataManagerExtensions
    {
        public static Task<ContentItemMetadata> GetContentItemMetadataAsync(this IContentManager contentManager, IContent content)
        {
            return contentManager.PopulateAspectAsync<ContentItemMetadata>(content);
        }
    }
}

