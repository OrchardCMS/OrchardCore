using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.ContentManagement
{
    public static class ContentRazorHelperExtensions
    {
        /// <summary>
        /// Returns a content item id from an alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <example>GetContentItemIdByAliasAsync("alias:carousel")</example>
        /// <example>GetContentItemIdByAliasAsync("autoroute:myblog/my-blog-post")</example>
        /// <returns>A content item id or <c>null</c> if it was not found.</returns>
        public static Task<string> GetContentItemIdByAliasAsync(this OrchardRazorHelper razorHelper, string alias)
        {
            var contentAliasManager = razorHelper.HttpContext.RequestServices.GetService<IContentAliasManager>();
            return contentAliasManager.GetContentItemIdAsync(alias);
        }

        /// <summary>
        /// Loads a content item by its alias.
        /// </summary>
        /// <param name="alias">The alias to load.</param>
        /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
        /// <example>GetContentItemByAliasAsync("alias:carousel")</example>
        /// <example>GetContentItemByAliasAsync("autoroute:myblog/my-blog-post", true)</example>
        /// <returns>A content item with the specific alias, or <c>null</c> if it doesn't exist.</returns>
        public static async Task<ContentItem> GetContentItemByAliasAsync(this OrchardRazorHelper razorHelper, string alias, bool latest = false)
        {
            var contentItemId = await GetContentItemIdByAliasAsync(razorHelper, alias);
            var contentManager = razorHelper.HttpContext.RequestServices.GetService<IContentManager>();
            return await contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
        }

        /// <summary>
        /// Loads a content item by its id.
        /// </summary>
        /// <param name="contentItemId">The content item id to load.</param>
        /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
        /// <example>GetContentItemByIdAsync("4xxxxxxxxxxxxxxxx")</example>
        /// <returns>A content item with the specific id, or <c>null</c> if it doesn't exist.</returns>
        public static Task<ContentItem> GetContentItemByIdAsync(this OrchardRazorHelper razorHelper, string contentItemId, bool latest = false)
        {
            var contentManager = razorHelper.HttpContext.RequestServices.GetService<IContentManager>();
            return contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
        }

        /// <summary>
        /// Loads a content item by its version id.
        /// </summary>
        /// <param name="contentItemVersionId">The content item version id to load.</param>
        /// <example>GetContentItemByVersionIdAsync("4xxxxxxxxxxxxxxxx")</example>
        /// <returns>A content item with the specific version id, or <c>null</c> if it doesn't exist.</returns>
        public static Task<ContentItem> GetContentItemByVersionIdAsync(this OrchardRazorHelper razorHelper, string contentItemVersionId)
        {
            var contentManager = razorHelper.HttpContext.RequestServices.GetService<IContentManager>();
            return contentManager.GetVersionAsync(contentItemVersionId);
        }
    }
}
