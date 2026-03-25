using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for Content shapes.
/// Alternates are computed once per unique content type / display type combination and cached for reuse.
/// Content item ID alternates are assembled at call time since the item ID space is unbounded.
/// </summary>
internal static class ContentShapeAlternatesFactory
{
    private static readonly ConcurrentDictionary<ContentAlternatesCacheKey, ContentAlternatesCacheEntry> _cache = new();

    /// <summary>
    /// Gets or creates a cached entry for the given content type and display type combination.
    /// The caller is responsible for assembling the content item ID alternates.
    /// </summary>
    internal static ContentAlternatesCacheEntry GetEntry(string contentType, string? displayType)
    {
        var key = new ContentAlternatesCacheKey(contentType, displayType ?? string.Empty);
        return _cache.GetOrAdd(key, static k => BuildEntry(k));
    }

    internal readonly record struct ContentAlternatesCacheKey(
        string ContentType,
        string DisplayType);

    internal sealed class ContentAlternatesCacheEntry(string displayTypeAlternate, string contentTypeAlternate, string displayTypeContentTypeAlternate, string displayTypePrefix)
    {
        // Content_[EncodedDisplayType]
        public string DisplayTypeAlternate { get; } = displayTypeAlternate;

        // Content__[EncodedContentType]
        public string ContentTypeAlternate { get; } = contentTypeAlternate;

        // Content_[DisplayType]__[EncodedContentType]
        public string DisplayTypeContentTypeAlternate { get; } = displayTypeContentTypeAlternate;

        // "Content_[DisplayType]__" — prefix for Content_[DisplayType]__[Id]
        public string DisplayTypePrefix { get; } = displayTypePrefix;

        internal IEnumerable<string> GetAlternates(string contentItemId)
        {
            yield return DisplayTypeAlternate;
            yield return ContentTypeAlternate;
            yield return "Content__" + contentItemId;
            yield return DisplayTypeContentTypeAlternate;
            yield return DisplayTypePrefix + contentItemId;
        }
    }

    private static ContentAlternatesCacheEntry BuildEntry(ContentAlternatesCacheKey key)
    {
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        return new ContentAlternatesCacheEntry(
            displayTypeAlternate: "Content_" + key.DisplayType.EncodeAlternateElement(),
            contentTypeAlternate: "Content__" + encodedContentType,
            displayTypeContentTypeAlternate: "Content_" + key.DisplayType + "__" + encodedContentType,
            displayTypePrefix: "Content_" + key.DisplayType + "__");
    }
}
