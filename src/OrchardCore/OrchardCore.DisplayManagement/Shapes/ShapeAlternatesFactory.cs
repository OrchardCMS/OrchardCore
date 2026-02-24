using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// Provides cached alternate patterns for shapes like Menu, MenuItem, Term, etc.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
public static class ShapeAlternatesFactory
{
    private static readonly ConcurrentDictionary<MenuItemAlternatesCacheKey, MenuItemAlternatesCollection> _menuItemCache = new();
    private static readonly ConcurrentDictionary<TermItemAlternatesCacheKey, TermItemAlternatesCollection> _termItemCache = new();
    private static readonly ConcurrentDictionary<ContentAlternatesCacheKey, ContentAlternatesCollection> _contentCache = new();

    /// <summary>
    /// Gets or creates cached alternates for a MenuItem shape configuration.
    /// </summary>
    public static MenuItemAlternatesCollection GetMenuItemAlternates(string contentType, string differentiator, int level)
    {
        var key = new MenuItemAlternatesCacheKey(contentType, differentiator ?? string.Empty, level);
        return _menuItemCache.GetOrAdd(key, static k => new MenuItemAlternatesCollection(k));
    }

    /// <summary>
    /// Gets or creates cached alternates for a TermItem/TermContentItem shape configuration.
    /// </summary>
    public static TermItemAlternatesCollection GetTermItemAlternates(string contentType, string differentiator, int level)
    {
        var key = new TermItemAlternatesCacheKey(contentType, differentiator ?? string.Empty, level);
        return _termItemCache.GetOrAdd(key, static k => new TermItemAlternatesCollection(k));
    }

    /// <summary>
    /// Gets or creates cached alternates for a Content shape configuration.
    /// </summary>
    public static ContentAlternatesCollection GetContentAlternates(string contentType, string contentItemId)
    {
        var key = new ContentAlternatesCacheKey(contentType, contentItemId);
        return _contentCache.GetOrAdd(key, static k => new ContentAlternatesCollection(k));
    }

    #region MenuItem Alternates

    internal readonly record struct MenuItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    /// <summary>
    /// Pre-computed alternates collection for a MenuItem configuration.
    /// </summary>
    public sealed class MenuItemAlternatesCollection
    {
        private readonly MenuItemAlternatesCacheKey _key;
        private FrozenSet<string> _menuItemAlternates;
        private FrozenSet<string> _menuItemLinkAlternates;

        internal MenuItemAlternatesCollection(MenuItemAlternatesCacheKey key)
        {
            _key = key;
        }

        /// <summary>
        /// Gets the cached alternates for MenuItem shapes.
        /// </summary>
        public FrozenSet<string> MenuItemAlternates => _menuItemAlternates ??= BuildMenuItemAlternates();

        /// <summary>
        /// Gets the cached alternates for MenuItemLink shapes.
        /// </summary>
        public FrozenSet<string> MenuItemLinkAlternates => _menuItemLinkAlternates ??= BuildMenuItemLinkAlternates();

        private FrozenSet<string> BuildMenuItemAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = EncodeAlternateElement(_key.ContentType);

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

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }

        private FrozenSet<string> BuildMenuItemLinkAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = EncodeAlternateElement(_key.ContentType);

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
                // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-MainMenu-HtmlMenuItem-level-2
                alternates.Add("MenuItemLink__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("MenuItemLink__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    #endregion

    #region TermItem Alternates

    internal readonly record struct TermItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    /// <summary>
    /// Pre-computed alternates collection for a TermItem/TermContentItem configuration.
    /// </summary>
    public sealed class TermItemAlternatesCollection
    {
        private readonly TermItemAlternatesCacheKey _key;
        private FrozenSet<string> _termItemAlternates;
        private FrozenSet<string> _termContentItemAlternates;

        internal TermItemAlternatesCollection(TermItemAlternatesCacheKey key)
        {
            _key = key;
        }

        /// <summary>
        /// Gets the cached alternates for TermItem shapes.
        /// </summary>
        public FrozenSet<string> TermItemAlternates => _termItemAlternates ??= BuildTermItemAlternates();

        /// <summary>
        /// Gets the cached alternates for TermContentItem shapes.
        /// </summary>
        public FrozenSet<string> TermContentItemAlternates => _termContentItemAlternates ??= BuildTermContentItemAlternates();

        private FrozenSet<string> BuildTermItemAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = EncodeAlternateElement(_key.ContentType);

            // TermItem__level__[level] e.g. TermItem-level-2
            alternates.Add("TermItem__level__" + _key.Level);

            // TermItem__[ContentType] e.g. TermItem-Category
            // TermItem__[ContentType]__level__[level] e.g. TermItem-Category-level-2
            alternates.Add("TermItem__" + encodedContentType);
            alternates.Add("TermItem__" + encodedContentType + "__level__" + _key.Level);

            if (!string.IsNullOrEmpty(_key.Differentiator))
            {
                // TermItem__[Differentiator] e.g. TermItem-Categories, TermItem-Travel
                // TermItem__[Differentiator]__level__[level] e.g. TermItem-Categories-level-2
                alternates.Add("TermItem__" + _key.Differentiator);
                alternates.Add("TermItem__" + _key.Differentiator + "__level__" + _key.Level);

                // TermItem__[Differentiator]__[ContentType] e.g. TermItem-Categories-Category
                // TermItem__[Differentiator]__[ContentType]__level__[level] e.g. TermItem-Categories-Category-level-2
                alternates.Add("TermItem__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("TermItem__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }

        private FrozenSet<string> BuildTermContentItemAlternates()
        {
            var alternates = new List<string>();
            var encodedContentType = EncodeAlternateElement(_key.ContentType);

            alternates.Add("TermContentItem__level__" + _key.Level);

            // TermContentItem__[ContentType] e.g. TermContentItem-Category
            // TermContentItem__[ContentType]__level__[level] e.g. TermContentItem-Category-level-2
            alternates.Add("TermContentItem__" + encodedContentType);
            alternates.Add("TermContentItem__" + encodedContentType + "__level__" + _key.Level);

            if (!string.IsNullOrEmpty(_key.Differentiator))
            {
                // TermContentItem__[Differentiator] e.g. TermContentItem-Categories
                alternates.Add("TermContentItem__" + _key.Differentiator);
                // TermContentItem__[Differentiator]__level__[level] e.g. TermContentItem-Categories-level-2
                alternates.Add("TermContentItem__" + _key.Differentiator + "__level__" + _key.Level);

                // TermContentItem__[Differentiator]__[ContentType] e.g. TermContentItem-Categories-Category
                // TermContentItem__[Differentiator]__[ContentType] e.g. TermContentItem-Categories-Category-level-2
                alternates.Add("TermContentItem__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("TermContentItem__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    #endregion

    #region Content Alternates

    internal readonly record struct ContentAlternatesCacheKey(
        string ContentType,
        string ContentItemId);

    /// <summary>
    /// Pre-computed alternates collection for a Content shape configuration.
    /// </summary>
    public sealed class ContentAlternatesCollection
    {
        private readonly ContentAlternatesCacheKey _key;
        private readonly ConcurrentDictionary<string, FrozenSet<string>> _alternatesByDisplayType = new(StringComparer.OrdinalIgnoreCase);

        internal ContentAlternatesCollection(ContentAlternatesCacheKey key)
        {
            _key = key;
        }

        /// <summary>
        /// Gets the cached alternates for a specific display type.
        /// </summary>
        public FrozenSet<string> GetAlternates(string displayType)
        {
            return _alternatesByDisplayType.GetOrAdd(displayType, BuildAlternates);
        }

        private FrozenSet<string> BuildAlternates(string displayType)
        {
            var alternates = new List<string>();
            var encodedContentType = EncodeAlternateElement(_key.ContentType);
            var encodedDisplayType = EncodeAlternateElement(displayType);

            // Content__[DisplayType] e.g. Content-Summary
            alternates.Add("Content_" + encodedDisplayType);

            // Content__[ContentType] e.g. Content-BlogPost
            alternates.Add("Content__" + encodedContentType);

            // Content__[Id] e.g. Content-42
            alternates.Add("Content__" + _key.ContentItemId);

            // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
            alternates.Add("Content_" + displayType + "__" + encodedContentType);

            // Content_[DisplayType]__[Id] e.g. Content-42.Summary
            alternates.Add("Content_" + displayType + "__" + _key.ContentItemId);

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    #endregion

    /// <summary>
    /// Encodes a string for use in an alternate element name.
    /// Replaces '-' with "__" to avoid conflicts with alternate separators.
    /// </summary>
    private static string EncodeAlternateElement(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Replace("-", "__");
    }
}
