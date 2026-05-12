using System.Collections.Concurrent;

namespace OrchardCore.DisplayManagement.Zones;

/// <summary>
/// Provides cached alternate patterns for Zone shapes.
/// Alternates are computed once per unique zone name and cached for reuse.
/// </summary>
internal static class ZoneAlternatesFactory
{
    private static readonly ConcurrentDictionary<string, string[]> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or creates cached alternate for a Zone shape configuration.
    /// </summary>
    public static string[] GetAlternates(string zoneName)
    {
        return _cache.GetOrAdd(zoneName, static z => [$"Zone__{z}"]);
    }
}
