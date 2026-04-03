using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Menu;

/// <summary>
/// Provides cached alternate patterns for MenuItem and MenuItemLink shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class MenuItemAlternatesFactory
{
    private static readonly ConcurrentDictionary<MenuItemAlternatesCacheKey, string[]> _menuItemCache = new();
    private static readonly ConcurrentDictionary<MenuItemAlternatesCacheKey, string[]> _menuItemLinkCache = new();

    /// <summary>
    /// Gets or creates cached alternates for a MenuItem shape configuration.
    /// </summary>
    public static string[] GetMenuItemAlternates(string contentType, string differentiator, int level)
    {
        var key = new MenuItemAlternatesCacheKey(contentType ?? string.Empty, differentiator ?? string.Empty, level);
        return _menuItemCache.GetOrAdd(key, BuildMenuItemAlternates);
    }

    public static string[] GetMenuItemLinkAlternates(string contentType, string differentiator, int level)
    {
        var key = new MenuItemAlternatesCacheKey(contentType ?? string.Empty, differentiator ?? string.Empty, level);
        return _menuItemLinkCache.GetOrAdd(key, BuildMenuItemLinkAlternates);
    }

    internal readonly record struct MenuItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    private static string[] BuildMenuItemAlternates(MenuItemAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        // MenuItem__level__[level] e.g. MenuItem-level-2
        alternates.Add("MenuItem__level__" + key.Level);

        // MenuItem__[ContentType] e.g. MenuItem-HtmlMenuItem
        // MenuItem__[ContentType]__level__[level] e.g. MenuItem-HtmlMenuItem-level-2
        alternates.Add("MenuItem__" + encodedContentType);
        alternates.Add("MenuItem__" + encodedContentType + "__level__" + key.Level);

        if (!string.IsNullOrEmpty(key.Differentiator))
        {
            // MenuItem__[MenuName] e.g. MenuItem-MainMenu
            // MenuItem__[MenuName]__level__[level] e.g. MenuItem-MainMenu-level-2
            alternates.Add("MenuItem__" + key.Differentiator);
            alternates.Add("MenuItem__" + key.Differentiator + "__level__" + key.Level);

            // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-MainMenu-HtmlMenuItem
            // MenuItem__[MenuName]__[ContentType]__level__[level] e.g. MenuItem-MainMenu-HtmlMenuItem-level-2
            alternates.Add("MenuItem__" + key.Differentiator + "__" + encodedContentType);
            alternates.Add("MenuItem__" + key.Differentiator + "__" + encodedContentType + "__level__" + key.Level);
        }

        return alternates.ToArray();
    }

    private static string[] BuildMenuItemLinkAlternates(MenuItemAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        alternates.Add("MenuItemLink__level__" + key.Level);

        // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
        // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
        alternates.Add("MenuItemLink__" + encodedContentType);
        alternates.Add("MenuItemLink__" + encodedContentType + "__level__" + key.Level);

        if (!string.IsNullOrEmpty(key.Differentiator))
        {
            // MenuItemLink__[MenuName] e.g. MenuItemLink-MainMenu
            // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-MainMenu-level-2
            alternates.Add("MenuItemLink__" + key.Differentiator);
            alternates.Add("MenuItemLink__" + key.Differentiator + "__level__" + key.Level);

            // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-MainMenu-HtmlMenuItem
            // MenuItemLink__[MenuName]__[ContentType]__level__[level] e.g. MenuItemLink-MainMenu-HtmlMenuItem-level-2
            alternates.Add("MenuItemLink__" + key.Differentiator + "__" + encodedContentType);
            alternates.Add("MenuItemLink__" + key.Differentiator + "__" + encodedContentType + "__level__" + key.Level);
        }

        return alternates.ToArray();
    }
}
