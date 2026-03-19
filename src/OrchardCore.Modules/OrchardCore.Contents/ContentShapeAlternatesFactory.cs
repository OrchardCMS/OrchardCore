using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for Content shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentShapeAlternatesFactory
{
    private static readonly ConcurrentDictionary<ContentAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Content shape configuration.
    /// </summary>
    public static string[] GetAlternates(string contentType, string contentItemId, string displayType)
    {
        var key = new ContentAlternatesCacheKey(contentType, contentItemId, displayType ?? string.Empty);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    internal readonly record struct ContentAlternatesCacheKey(
        string ContentType,
        string ContentItemId,
        string DisplayType);

    private static string[] BuildAlternates(ContentAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        // Content__[DisplayType] e.g. Content-Summary
        alternates.Add("Content_" + key.DisplayType.EncodeAlternateElement());

        // Content__[ContentType] e.g. Content-BlogPost
        alternates.Add("Content__" + encodedContentType);

        // Content__[Id] e.g. Content-42
        alternates.Add("Content__" + key.ContentItemId);

        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
        alternates.Add("Content_" + key.DisplayType + "__" + encodedContentType);

        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
        alternates.Add("Content_" + key.DisplayType + "__" + key.ContentItemId);

        return alternates.ToArray();
    }
}
