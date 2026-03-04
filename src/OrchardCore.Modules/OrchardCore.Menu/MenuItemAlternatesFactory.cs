using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Menu;

/// <summary>
/// Provides cached alternate patterns for MenuItem and MenuItemLink shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class MenuItemAlternatesFactory
{
    private static readonly ConcurrentDictionary<MenuItemAlternatesCacheKey, MenuItemAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a MenuItem shape configuration.
    /// </summary>
    public static MenuItemAlternatesCollection GetAlternates(string contentType, string differentiator, int level)
    {
        var key = new MenuItemAlternatesCacheKey(contentType, differentiator ?? string.Empty, level);
        return _cache.GetOrAdd(key, static k => new MenuItemAlternatesCollection(k));
    }

    internal readonly record struct MenuItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    /// <summary>
    /// Pre-computed alternates collection for a MenuItem configuration.
    /// </summary>
    internal sealed class MenuItemAlternatesCollection
    {
        private readonly MenuItemAlternatesCacheKey _key;
        private string[] _menuItemAlternates;
        private string[] _menuItemLinkAlternates;

        internal MenuItemAlternatesCollection(MenuItemAlternatesCacheKey key)
        {
            _key = key;
        }

        /// <summary>
        /// Gets the cached alternates for MenuItem shapes.
        /// </summary>
        public string[] MenuItemAlternates => _menuItemAlternates ??= BuildMenuItemAlternates();

        /// <summary>
        /// Gets the cached alternates for MenuItemLink shapes.
        /// </summary>
        public string[] MenuItemLinkAlternates => _menuItemLinkAlternates ??= BuildMenuItemLinkAlternates();

        private string[] BuildMenuItemAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = _key.ContentType.EncodeAlternateElement();

            // MenuItem__level__[level] e.g. MenuItem-level-2
            alternates.Add("MenuItem__level__" + _key.Level);

            // MenuItem__[ContentType] e.g. MenuItem-HtmlMenuItem
            // MenuItem__[ContentType]__level__[level] e.g. MenuItem-HtmlMenuItem-level-2
            alternates.Add("MenuItem__" + encodedContentType);
            alternates.Add("MenuItem__" + encodedContentType + "__level__" + _key.Level);

            if (!string.IsNullOrEmpty(_key.Differentiator))
            {
                // MenuItem__[MenuName] e.g. MenuItem-MainMenu
                // MenuItem__[MenuName]__level__[level] e.g. MenuItem-MainMenu-level-2
                alternates.Add("MenuItem__" + _key.Differentiator);
                alternates.Add("MenuItem__" + _key.Differentiator + "__level__" + _key.Level);

                // MenuItem__[MenuName]__[ContentType] e.g. MenuItem-MainMenu-HtmlMenuItem
                // MenuItem__[MenuName]__[ContentType]__level__[level] e.g. MenuItem-MainMenu-HtmlMenuItem-level-2
                alternates.Add("MenuItem__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("MenuItem__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToArray();
        }

        private string[] BuildMenuItemLinkAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = _key.ContentType.EncodeAlternateElement();

            alternates.Add("MenuItemLink__level__" + _key.Level);

            // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
            // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
            alternates.Add("MenuItemLink__" + encodedContentType);
            alternates.Add("MenuItemLink__" + encodedContentType + "__level__" + _key.Level);

            if (!string.IsNullOrEmpty(_key.Differentiator))
            {
                // MenuItemLink__[MenuName] e.g. MenuItemLink-MainMenu
                // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-MainMenu-level-2
                alternates.Add("MenuItemLink__" + _key.Differentiator);
                alternates.Add("MenuItemLink__" + _key.Differentiator + "__level__" + _key.Level);

                // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-MainMenu-HtmlMenuItem
                // MenuItemLink__[MenuName]__[ContentType]__level__[level] e.g. MenuItemLink-MainMenu-HtmlMenuItem-level-2
                alternates.Add("MenuItemLink__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("MenuItemLink__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToArray();
        }
    }
}
