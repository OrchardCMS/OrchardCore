using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Services;

internal static class LayerSettingsEditor
{
    private static readonly char[] s_separators = [' ', ','];

    internal static string[] Parse(string zones) =>
        (zones ?? string.Empty).Split(s_separators, StringSplitOptions.RemoveEmptyEntries);

    internal static bool Apply(LayerSettings settings, IEnumerable<string> zones)
    {
        var normalized = zones.SelectMany(Parse).ToArray();
        if (settings.Zones is not null && settings.Zones.SequenceEqual(normalized, StringComparer.Ordinal))
        {
            return false;
        }

        settings.Zones = normalized;
        return true;
    }
}
