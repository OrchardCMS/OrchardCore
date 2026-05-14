using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Taxonomies;

/// <summary>
/// Provides cached alternate patterns for TermItem and TermContentItem shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class TermItemAlternatesFactory
{
    private static readonly ConcurrentDictionary<TermItemAlternatesCacheKey, string[]> _termItemCache = new();
    private static readonly ConcurrentDictionary<TermItemAlternatesCacheKey, string[]> _termContentItemCache = new();

    /// <summary>
    /// Gets or creates cached alternates for a TermItem/TermContentItem shape configuration.
    /// </summary>
    public static string[] GetTermItemAlternates(string contentType, string differentiator, int level)
    {
        var key = new TermItemAlternatesCacheKey(contentType ?? string.Empty, differentiator ?? string.Empty, level);
        return _termItemCache.GetOrAdd(key, BuildTermItemAlternates);
    }

    public static string[] GetTermContentItemAlternates(string contentType, string differentiator, int level)
    {
        var key = new TermItemAlternatesCacheKey(contentType ?? string.Empty, differentiator ?? string.Empty, level);
        return _termContentItemCache.GetOrAdd(key, BuildTermContentItemAlternates);
    }

    internal readonly record struct TermItemAlternatesCacheKey(
        string ContentType,
        string Differentiator,
        int Level);

    private static string[] BuildTermItemAlternates(TermItemAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        // TermItem__level__[level] e.g. TermItem-level-2
        alternates.Add("TermItem__level__" + key.Level);

        // TermItem__[ContentType] e.g. TermItem-Category
        // TermItem__[ContentType]__level__[level] e.g. TermItem-Category-level-2
        alternates.Add("TermItem__" + encodedContentType);
        alternates.Add("TermItem__" + encodedContentType + "__level__" + key.Level);

        if (!string.IsNullOrEmpty(key.Differentiator))
        {
            // TermItem__[Differentiator] e.g. TermItem-Categories, TermItem-Travel
            // TermItem__[Differentiator]__level__[level] e.g. TermItem-Categories-level-2
            alternates.Add("TermItem__" + key.Differentiator);
            alternates.Add("TermItem__" + key.Differentiator + "__level__" + key.Level);

            // TermItem__[Differentiator]__[ContentType] e.g. TermItem-Categories-Category
            // TermItem__[Differentiator]__[ContentType]__level__[level] e.g. TermItem-Categories-Category-level-2
            alternates.Add("TermItem__" + key.Differentiator + "__" + encodedContentType);
            alternates.Add("TermItem__" + key.Differentiator + "__" + encodedContentType + "__level__" + key.Level);
        }

        return alternates.ToArray();
    }

    private static string[] BuildTermContentItemAlternates(TermItemAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        alternates.Add("TermContentItem__level__" + key.Level);

        // TermContentItem__[ContentType] e.g. TermContentItem-Category
        // TermContentItem__[ContentType]__level__[level] e.g. TermContentItem-Category-level-2
        alternates.Add("TermContentItem__" + encodedContentType);
        alternates.Add("TermContentItem__" + encodedContentType + "__level__" + key.Level);

        if (!string.IsNullOrEmpty(key.Differentiator))
        {
            // TermContentItem__[Differentiator] e.g. TermContentItem-Categories
            alternates.Add("TermContentItem__" + key.Differentiator);
            // TermContentItem__[Differentiator]__level__[level] e.g. TermContentItem-Categories-level-2
            alternates.Add("TermContentItem__" + key.Differentiator + "__level__" + key.Level);

            // TermContentItem__[Differentiator]__[ContentType] e.g. TermContentItem-Categories-Category
            // TermContentItem__[Differentiator]__[ContentType]__level__[level] e.g. TermContentItem-Categories-Category-level-2
            alternates.Add("TermContentItem__" + key.Differentiator + "__" + encodedContentType);
            alternates.Add("TermContentItem__" + key.Differentiator + "__" + encodedContentType + "__level__" + key.Level);
        }

        return alternates.ToArray();
    }
}
