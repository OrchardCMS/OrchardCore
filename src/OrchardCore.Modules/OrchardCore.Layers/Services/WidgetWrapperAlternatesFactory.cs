using System.Collections.Concurrent;

namespace OrchardCore.Layers.Services;

/// <summary>
/// Provides cached alternate patterns for Widget_Wrapper shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class WidgetWrapperAlternatesFactory
{
    private static readonly ConcurrentDictionary<WidgetWrapperAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Widget_Wrapper shape configuration.
    /// </summary>
    public static string[] GetAlternates(string contentType, string zone)
    {
        var key = new WidgetWrapperAlternatesCacheKey(contentType, zone);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    private static string[] BuildAlternates(WidgetWrapperAlternatesCacheKey key)
    {
        return
        [
            // Widget_Wrapper__[ContentType] e.g. Widget_Wrapper__HtmlWidget
            $"Widget_Wrapper__{key.ContentType}",

            // Widget_Wrapper__Zone__[Zone] e.g. Widget_Wrapper__Zone__Sidebar
            $"Widget_Wrapper__Zone__{key.Zone}"
        ];
    }

    internal readonly record struct WidgetWrapperAlternatesCacheKey(
        string ContentType,
        string Zone);
}
