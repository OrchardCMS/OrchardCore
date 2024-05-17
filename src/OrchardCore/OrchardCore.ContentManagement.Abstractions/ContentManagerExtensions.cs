using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentManagement;

public static class ContentManagerExtensions
{
    /// <summary>
    /// Gets the published content items with the specified ids.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="contentItemId">The content item id to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <remarks>
    /// This method will always issue a database query.
    /// This means that it should be used only to get a list of content items that have not been loaded.
    /// </remarks>
    public static Task<ContentItem> GetAsync(this IContentManager contentManager, string contentItemId, bool latest = false)
        => contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);

    /// <summary>
    /// Gets the published content items with the specified ids.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="contentItemIds">The content item ids to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <remarks>
    /// This method will always issue a database query.
    /// This means that it should be used only to get a list of content items that have not been loaded.
    /// </remarks>
    public static Task<IEnumerable<ContentItem>> GetAsync(this IContentManager contentManager, IEnumerable<string> contentItemIds, bool latest = false)
        => contentManager.GetAsync(contentItemIds, latest ? VersionOptions.Latest : VersionOptions.Published);

    /// <summary>
    /// Gets the published content item with the specified id.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="contentItemId">The content item id to load.</param>
    public static Task<ContentItem> GetAsync(this IContentManager contentManager, string contentItemId)
        => contentManager.GetAsync(contentItemId, VersionOptions.Published);

    /// <summary>
    /// Creates (persists) a new Published content item.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="contentItem">The content instance filled with all necessary data.</param>
    public static Task CreateAsync(this IContentManager contentManager, ContentItem contentItem)
    {
        return contentManager.CreateAsync(contentItem, VersionOptions.Published);
    }

    public static Task<TAspect> PopulateAspectAsync<TAspect>(this IContentManager contentManager, IContent content) where TAspect : new()
    {
        return contentManager.PopulateAspectAsync(content, new TAspect());
    }

    public static async Task<bool> HasPublishedVersionAsync(this IContentManager contentManager, IContent content)
    {
        if (content.ContentItem == null)
        {
            return false;
        }

        return content.ContentItem.IsPublished() || (await contentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Published) != null);
    }

    public static Task<ContentItemMetadata> GetContentItemMetadataAsync(this IContentManager contentManager, IContent content)
    {
        return contentManager.PopulateAspectAsync<ContentItemMetadata>(content);
    }

    public static async Task<IEnumerable<ContentItem>> LoadAsync(this IContentManager contentManager, IEnumerable<ContentItem> contentItems)
    {
        var results = new List<ContentItem>(contentItems.Count());

        foreach (var contentItem in contentItems)
        {
            results.Add(await contentManager.LoadAsync(contentItem));
        }

        return results;
    }

    public static async IAsyncEnumerable<ContentItem> LoadAsync(this IContentManager contentManager, IAsyncEnumerable<ContentItem> contentItems)
    {
        await foreach (var contentItem in contentItems)
        {
            yield return await contentManager.LoadAsync(contentItem);
        }
    }

    public static async Task<ContentValidateResult> UpdateValidateAndCreateAsync(this IContentManager contentManager, ContentItem contentItem, VersionOptions options)
    {
        await contentManager.UpdateAsync(contentItem);
        var result = await contentManager.ValidateAsync(contentItem);

        if (result.Succeeded)
        {
            await contentManager.CreateAsync(contentItem, options);
        }

        return result;
    }

    /// <summary>
    /// Gets either the published container content item with the specified id, or if the json path supplied gets the contained content item. 
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="id">The content item id to load.</param>
    /// <param name="jsonPath">The json path of the contained content item.</param>
    public static Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath)
    {
        return contentManager.GetAsync(id, jsonPath, VersionOptions.Published);
    }

    /// <summary>
    /// Gets either the container content item with the specified id and version, or if the json path supplied gets the contained content item.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="id">The id content item id to load.</param>
    /// <param name="options">The version option.</param>
    /// <param name="jsonPath">The json path of the contained content item.</param>
    public static async Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath, VersionOptions options)
    {
        var contentItem = await contentManager.GetAsync(id, options);

        // It represents a contained content item.
        if (!string.IsNullOrEmpty(jsonPath))
        {
            var root = (JsonObject)contentItem.Content;
            contentItem = root.SelectNode(jsonPath)?.ToObject<ContentItem>();

            return contentItem;
        }

        return contentItem;
    }
}
