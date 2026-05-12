using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Autoroute.Services;

/// <summary>
/// Provides cached alternate patterns for Content shapes with Autoroute.
/// Alternates are computed once per unique slug and display type combination and cached for reuse.
/// </summary>
internal static class AutorouteAlternatesFactory
{
    private static readonly ConcurrentDictionary<AutorouteAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Content shape with Autoroute configuration.
    /// </summary>
    public static string[] GetAlternates(string path, string displayType)
    {
        var key = new AutorouteAlternatesCacheKey(path, displayType);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    private static string[] BuildAlternates(AutorouteAlternatesCacheKey key)
    {
        var encodedSlug = key.Path.EncodeAlternateElement().Replace("/", "__");

        return
        [
            // Content__Slug__[Slug] e.g. Content-Slug-example, Content-Slug-blog-my-post
            $"Content__Slug__{encodedSlug}",

            // Content_[DisplayType]__Slug__[Slug] e.g. Content-Slug-example.Summary, Content-Slug-blog-my-post.Summary
            $"Content_{key.DisplayType}__Slug__{encodedSlug}"
        ];
    }

    internal readonly record struct AutorouteAlternatesCacheKey(
        string Path,
        string DisplayType);
}
