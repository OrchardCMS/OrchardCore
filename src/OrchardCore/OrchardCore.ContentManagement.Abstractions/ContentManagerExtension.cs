using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
{
    public static class ContentManagerExtension
    {
        public static async Task UpdateAndCreateAsync(this IContentManager contentManager, ContentItem contentItem, VersionOptions options)
        {
            await contentManager.UpdateAsync(contentItem);
            await contentManager.CreateAsync(contentItem, options);
        }

        /// <summary>
        /// Gets either the published container content item with the specified id, or if the json path supplied gets the contained content item. 
        /// </summary>
        /// <param name="id">The content item id to load</param>
        /// <param name="jsonPath">The json path of the contained content item</param>
        public static Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath)
        {
            return contentManager.GetAsync(id, jsonPath, VersionOptions.Latest);
        }

        /// <summary>
        /// Gets either the container content item with the specified id and version, or if the json path supplied gets the contained content item.
        /// </summary>
        /// <param name="id">The id content item id to load</param>
        /// <param name="options">The version option</param>
        /// <param name="jsonPath">The json path of the contained content item</param>
        public static async Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath, VersionOptions options)
        {
            var contentItem = await contentManager.GetAsync(id, options);

            // It represents a contained content item
            if (!string.IsNullOrEmpty(jsonPath))
            {
                var root = contentItem.Content as JObject;
                contentItem = root.SelectToken(jsonPath)?.ToObject<ContentItem>();

                return contentItem;
            }

            return contentItem;
        }
    }
}
