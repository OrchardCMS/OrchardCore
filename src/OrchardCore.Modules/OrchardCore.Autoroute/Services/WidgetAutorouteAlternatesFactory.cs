using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Autoroute.Services;

/// <summary>
/// Provides cached alternate patterns for Widget shapes with Autoroute.
/// Alternates are computed once per unique slug and display type combination and cached for reuse.
/// </summary>
internal static class WidgetAutorouteAlternatesFactory
{
    private static readonly ConcurrentDictionary<WidgetAutorouteAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Widget shape with Autoroute configuration.
    /// </summary>
    public static string[] GetAlternates(string path, string displayType)
    {
        var key = new WidgetAutorouteAlternatesCacheKey(path, displayType);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    private static string[] BuildAlternates(WidgetAutorouteAlternatesCacheKey key)
    {
        var encodedSlug = key.Path.EncodeAlternateElement().Replace("/", "__");

        return
        [
            // Widget__Slug__[Slug] e.g. Widget-Slug-example, Widget-Slug-blog-my-post
            $"Widget__Slug__{encodedSlug}",

            // Widget_[DisplayType]__Slug__[Slug] e.g. Widget-Slug-example.Summary, Widget-Slug-blog-my-post.Summary
            $"Widget_{key.DisplayType}__Slug__{encodedSlug}"
        ];
    }

    internal readonly record struct WidgetAutorouteAlternatesCacheKey(
        string Path,
        string DisplayType);
}
