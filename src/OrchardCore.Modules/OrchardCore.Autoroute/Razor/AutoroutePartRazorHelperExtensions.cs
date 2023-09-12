using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

#pragma warning disable CA1050 // Declare types in namespaces
public static class AutoroutePartRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a content item id by its slug.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="slug">The slug.</param>
    /// <example>GetContentItemIdBySlugAsync("myblog/my-blog-post")</example>
    /// <returns>A content item id or <c>null</c> if it was not found.</returns>
    public static async Task<string> GetContentItemIdBySlugAsync(this IOrchardHelper orchardHelper, string slug)
    {
        if (String.IsNullOrEmpty(slug))
        {
            return null;
        }

        // Provided for backwards compatability and avoiding confusion.
        if (slug.StartsWith("slug:", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[5..];
        }

        if (!slug.StartsWith('/'))
        {
            slug = "/" + slug;
        }

        var autorouteEntries = orchardHelper.HttpContext.RequestServices.GetService<IAutorouteEntries>();

        (var found, var entry) = await autorouteEntries.TryGetEntryByPathAsync(slug);

        if (found)
        {
            return entry.ContentItemId;
        }

        return null;
    }

    /// <summary>
    /// Loads a content item by its slug.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="slug">The slug to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <example>GetContentItemBySlugAsync("myblog/my-blog-post")</example>
    /// <returns>A content item with the specific name, or <c>null</c> if it doesn't exist.</returns>
    public static async Task<ContentItem> GetContentItemBySlugAsync(this IOrchardHelper orchardHelper, string slug, bool latest = false)
    {
        var contentItemId = await orchardHelper.GetContentItemIdBySlugAsync(slug);
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();

        return await contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
    }
}
