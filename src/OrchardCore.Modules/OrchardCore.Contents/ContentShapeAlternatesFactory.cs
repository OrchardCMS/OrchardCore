using System.Collections.Concurrent;
using System.Collections.Frozen;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for Content shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentShapeAlternatesFactory
{
    private static readonly ConcurrentDictionary<ContentAlternatesCacheKey, ContentAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Content shape configuration.
    /// </summary>
    public static ContentAlternatesCollection GetAlternates(string contentType, string contentItemId)
    {
        var key = new ContentAlternatesCacheKey(contentType, contentItemId);
        return _cache.GetOrAdd(key, static k => new ContentAlternatesCollection(k));
    }

    internal readonly record struct ContentAlternatesCacheKey(
        string ContentType,
        string ContentItemId);

    /// <summary>
    /// Pre-computed alternates collection for a Content shape configuration.
    /// </summary>
    internal sealed class ContentAlternatesCollection
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
            var encodedContentType = _key.ContentType.EncodeAlternateElement();

            // Content__[DisplayType] e.g. Content-Summary
            alternates.Add("Content_" + displayType.EncodeAlternateElement());

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
}
