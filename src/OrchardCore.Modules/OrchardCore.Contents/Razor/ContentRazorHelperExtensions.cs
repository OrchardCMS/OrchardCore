using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Returns a content item id from an alias.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <example>GetContentItemIdByAliasAsync("alias:carousel")</example>
    /// <example>GetContentItemIdByAliasAsync("autoroute:myblog/my-blog-post")</example>
    /// <returns>A content item id or <c>null</c> if it was not found.</returns>
    public static Task<string> GetContentItemIdByAliasAsync(this IOrchardHelper orchardHelper, string alias)
    {
        var contentAliasManager = orchardHelper.HttpContext.RequestServices.GetService<IContentAliasManager>();
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
    public static async Task<ContentItem> GetContentItemByAliasAsync(this IOrchardHelper orchardHelper, string alias, bool latest = false)
    {
        var contentItemId = await GetContentItemIdByAliasAsync(orchardHelper, alias);
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        return await contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
    }

    /// <summary>
    /// Loads a content item by its id.
    /// </summary>
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
    /// <param name="contentType">The content type to load.</param>
    public static Task<IEnumerable<ContentItem>> GetRecentContentItemsByContentTypeAsync(this IOrchardHelper orchardHelper, string contentType, int maxContentItems = 10)
    {
        return orchardHelper.QueryContentItemsAsync(query => query.Where(x => x.ContentType == contentType && x.Published == true).OrderByDescending(x => x.CreatedUtc).Take(maxContentItems));
    }
}