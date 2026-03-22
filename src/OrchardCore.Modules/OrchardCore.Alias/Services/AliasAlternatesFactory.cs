using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Alias.Services;

/// <summary>
/// Provides cached alternate patterns for Content shapes with Alias.
/// Alternates are computed once per unique alias and display type combination and cached for reuse.
/// </summary>
internal static class AliasAlternatesFactory
{
    private static readonly ConcurrentDictionary<AliasAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a Content shape with Alias configuration.
    /// </summary>
    public static string[] GetAlternates(string alias, string displayType)
    {
        var key = new AliasAlternatesCacheKey(alias, displayType);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    private static string[] BuildAlternates(AliasAlternatesCacheKey key)
    {
        var encodedAlias = key.Alias.EncodeAlternateElement();

        return
        [
            // Content__Alias__[Alias] e.g. Content-Alias-example, Content-Alias-my-page
            $"Content__Alias__{encodedAlias}",

            // Content_[DisplayType]__Alias__[Alias] e.g. Content-Alias-example.Summary, Content-Alias-my-page.Summary
            $"Content_{key.DisplayType}__Alias__{encodedAlias}"
        ];
    }

    internal readonly record struct AliasAlternatesCacheKey(
        string Alias,
        string DisplayType);
}
