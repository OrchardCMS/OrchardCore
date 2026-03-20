using System.Collections.Concurrent;

namespace OrchardCore.Menu;

/// <summary>
/// Provides cached alternate patterns for Menu shapes.
/// Alternates are computed once per unique menu name and cached for reuse.
/// </summary>
internal static class MenuAlternatesFactory
{
    private static readonly ConcurrentDictionary<string, string[]> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or creates cached alternate for a Menu shape.
    /// </summary>
    public static string[] GetMenuAlternates(string differentiator)
    {
        return _cache.GetOrAdd(differentiator, static d => [$"Menu__{d}"]);
    }
}
