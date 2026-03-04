using System.Collections.Concurrent;
using System.Collections.Frozen;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Taxonomies;

/// <summary>
/// Provides cached alternate patterns for TermItem and TermContentItem shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class TermItemAlternatesFactory
{
    private static readonly ConcurrentDictionary<TermItemAlternatesCacheKey, TermItemAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a TermItem/TermContentItem shape configuration.
    /// </summary>
    public static TermItemAlternatesCollection GetAlternates(string contentType, string differentiator, int level)
    {
        var key = new TermItemAlternatesCacheKey(contentType, differentiator ?? string.Empty, level);
        return _cache.GetOrAdd(key, static k => new TermItemAlternatesCollection(k));
    }

    internal readonly record struct TermItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    /// <summary>
    /// Pre-computed alternates collection for a TermItem/TermContentItem configuration.
    /// </summary>
    internal sealed class TermItemAlternatesCollection
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
            var encodedContentType = _key.ContentType.EncodeAlternateElement();

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
            var encodedContentType = _key.ContentType.EncodeAlternateElement();

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
                // TermContentItem__[Differentiator]__[ContentType]__level__[level] e.g. TermContentItem-Categories-Category-level-2
                alternates.Add("TermContentItem__" + _key.Differentiator + "__" + encodedContentType);
                alternates.Add("TermContentItem__" + _key.Differentiator + "__" + encodedContentType + "__level__" + _key.Level);
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
