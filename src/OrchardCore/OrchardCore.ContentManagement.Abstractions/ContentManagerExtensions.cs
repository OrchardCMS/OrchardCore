using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentManagement;

public static class ContentManagerExtensions
{
    public static Task<TAspect> PopulateAspectAsync<TAspect>(this IContentManager contentManager, IContent content) where TAspect : new()
    {
        return contentManager.PopulateAspectAsync(content, new TAspect());
    }

    public static async Task<bool> HasPublishedVersionAsync(this IContentManager contentManager, IContent content)
    {
        if (content?.ContentItem == null)
        {
            return false;
        }

        return content.ContentItem.IsPublished() ||
            (await contentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Published) != null);
    }

    public static Task<ContentItemMetadata> GetContentItemMetadataAsync(this IContentManager contentManager, IContent content)
    {
        return contentManager.PopulateAspectAsync<ContentItemMetadata>(content);
    }

    public static async Task<IEnumerable<ContentItem>> LoadAsync(this IContentManager contentManager, IEnumerable<ContentItem> contentItems)
    {
        ArgumentNullException.ThrowIfNull(contentItems);

        var results = new List<ContentItem>(contentItems.Count());

        foreach (var contentItem in contentItems)
        {
            results.Add(await contentManager.LoadAsync(contentItem));
        }

        return results;
    }

    public static async IAsyncEnumerable<ContentItem> LoadAsync(this IContentManager contentManager, IAsyncEnumerable<ContentItem> contentItems)
    {
        ArgumentNullException.ThrowIfNull(contentItems);

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
    /// Gets either the container content item with the specified id and version, or if the json path supplied gets the contained content item.
    /// </summary>
    /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
    /// <param name="contentItemId">The id content item id to load.</param>
    /// <param name="options">The version option.</param>
    /// <param name="jsonPath">The json path of the contained content item.</param>
    public static async Task<ContentItem> GetAsync(this IContentManager contentManager, string contentItemId, string jsonPath, VersionOptions options = null)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, options);

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
