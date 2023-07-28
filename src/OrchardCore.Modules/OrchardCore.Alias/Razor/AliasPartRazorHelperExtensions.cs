using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Alias.Services;
using OrchardCore.ContentManagement;
using YesSql;

#pragma warning disable CA1050 // Declare types in namespaces
public static class AliasPartRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a content item id by its alias.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="alias">The alias.</param>
    /// <example>GetContentItemIdByAliasAsync("carousel")</example>
    /// <returns>A content item id or <c>null</c> if it was not found.</returns>
    public static async Task<string> GetContentItemIdByAliasAsync(this IOrchardHelper orchardHelper, string alias)
    {
        if (String.IsNullOrEmpty(alias))
        {
            return null;
        }

        // Provided for backwards compatability and avoiding confusion.
        if (alias.StartsWith("alias:", StringComparison.OrdinalIgnoreCase))
        {
            alias = alias[6..];
        }

        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();
        var aliasPartIndex = await AliasPartContentHandleHelper.QueryAliasIndex(session, alias);

        return aliasPartIndex?.ContentItemId;
    }

    /// <summary>
    /// Loads a content item by its alias.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="alias">The alias to load.</param>
    /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
    /// <example>GetContentItemIdByAliasAsync("carousel")</example>
    /// <returns>A content item with the specific name, or <c>null</c> if it doesn't exist.</returns>
    public static async Task<ContentItem> GetContentItemByAliasAsync(this IOrchardHelper orchardHelper, string alias, bool latest = false)
    {
        var contentItemId = await orchardHelper.GetContentItemIdByAliasAsync(alias);
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();

        return await contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
    }
}
