using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Alias.Services;

/// <summary>
/// Provides cached alternate patterns for Widget shapes with Alias.
/// Alternates are computed once per unique alias and display type combination and cached for reuse.
/// </summary>
internal static class WidgetAliasAlternatesFactory
{
    private static readonly ConcurrentDictionary<WidgetAliasAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Widget shape with Alias configuration.
    /// </summary>
    public static string[] GetAlternates(string alias, string displayType)
    {
        var key = new WidgetAliasAlternatesCacheKey(alias, displayType);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    private static string[] BuildAlternates(WidgetAliasAlternatesCacheKey key)
    {
        var encodedAlias = key.Alias.EncodeAlternateElement();

        return
        [
            // Widget__Alias__[Alias] e.g. Widget-Alias-example, Widget-Alias-my-page
            $"Widget__Alias__{encodedAlias}",

            // Widget_[DisplayType]__Alias__[Alias] e.g. Widget-Alias-example.Summary, Widget-Alias-my-page.Summary
            $"Widget_{key.DisplayType}__Alias__{encodedAlias}"
        ];
    }

    internal readonly record struct WidgetAliasAlternatesCacheKey(
        string Alias,
        string DisplayType);
}
