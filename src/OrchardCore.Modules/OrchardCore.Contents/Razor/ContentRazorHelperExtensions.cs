using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

#pragma warning disable CA1050 // Declare types in namespaces
public static class ContentRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a content item id by its handle.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="handle">The handle.</param>
    /// <example>GetContentItemIdByHandleAsync("alias:carousel")</example>
    /// <example>GetContentItemIdByHandleAsync("slug:myblog/my-blog-post")</example>
    /// <returns>A content item id or <c>null</c> if it was not found.</returns>
    public static Task<string> GetContentItemIdByHandleAsync(this IOrchardHelper orchardHelper, string handle)
    {
        var contentHandleManager = orchardHelper.HttpContext.RequestServices.GetService<IContentHandleManager>();
        return contentHandleManager.GetContentItemIdAsync(handle);
    }

    /// <summary>
    /// Loads a content item by its handle.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="handle">The handle to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <example>GetContentItemByHandleAsync("alias:carousel")</example>
    /// <example>GetContentItemByHandleAsync("slug:myblog/my-blog-post", true)</example>
    /// <returns>A content item with the specific name, or <c>null</c> if it doesn't exist.</returns>
    public static async Task<ContentItem> GetContentItemByHandleAsync(this IOrchardHelper orchardHelper, string handle, bool latest = false)
    {
        var contentItemId = await GetContentItemIdByHandleAsync(orchardHelper, handle);
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        return await contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
    }

    /// <summary>
    /// Loads a content item by its id.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="contentItemId">The content item id to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <example>GetContentItemByIdAsync("4xxxxxxxxxxxxxxxx")</example>
    /// <returns>A content item with the specific id, or <c>null</c> if it doesn't exist.</returns>
    public static Task<ContentItem> GetContentItemByIdAsync(this IOrchardHelper orchardHelper, string contentItemId, bool latest = false)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        return contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
    }

    /// <summary>
    /// Loads a list of content items by their ids.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="contentItemIds">The content item ids to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <returns>A list of content items with the specific ids.</returns>
    public static Task<IEnumerable<ContentItem>> GetContentItemsByIdAsync(this IOrchardHelper orchardHelper, IEnumerable<string> contentItemIds, bool latest = false)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        return contentManager.GetAsync(contentItemIds, latest);
    }

    /// <summary>
    /// Loads a content item by its version id.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="contentItemVersionId">The content item version id to load.</param>
    /// <example>GetContentItemByVersionIdAsync("4xxxxxxxxxxxxxxxx")</example>
    /// <returns>A content item with the specific version id, or <c>null</c> if it doesn't exist.</returns>
    public static Task<ContentItem> GetContentItemByVersionIdAsync(this IOrchardHelper orchardHelper, string contentItemVersionId)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        return contentManager.GetVersionAsync(contentItemVersionId);
    }

    /// <summary>
    /// Query content items.
    /// </summary>
    public static async Task<IEnumerable<ContentItem>> QueryContentItemsAsync(this IOrchardHelper orchardHelper, Func<IQuery<ContentItem, ContentItemIndex>, IQuery<ContentItem>> query)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

        var contentItems = await query(session.Query<ContentItem, ContentItemIndex>()).ListAsync();

        return await contentManager.LoadAsync(contentItems);
    }

    /// <summary>
    /// Loads content items of a specific type.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="contentType">The content type to load.</param>
    /// <param name="maxContentItems">The maximum content items to return.</param>
    public static Task<IEnumerable<ContentItem>> GetRecentContentItemsByContentTypeAsync(this IOrchardHelper orchardHelper, string contentType, int maxContentItems = 10)
    {
        return orchardHelper.QueryContentItemsAsync(query => query.Where(x => x.ContentType == contentType && x.Published == true).OrderByDescending(x => x.CreatedUtc).Take(maxContentItems));
    }
}
