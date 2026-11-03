using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Helpers;

public static class ActivityPresetProviderExtensions
{
    /// <summary>
    /// Returns the presets of every provider.
    /// </summary>
    public static async Task<IReadOnlyList<ActivityPreset>> ListPresetsAsync(this IEnumerable<IActivityPresetProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        var presets = new List<ActivityPreset>();

        foreach (var provider in providers)
        {
            presets.AddRange(await provider.GetPresetsAsync() ?? []);
        }

        return presets;
    }

    /// <summary>
    /// Returns the preset of an identifier, or <see langword="null"/>.
    /// </summary>
    public static async Task<ActivityPreset> FindPresetAsync(this IEnumerable<IActivityPresetProvider> providers, string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return (await providers.ListPresetsAsync()).FirstOrDefault(preset => preset.Id == id);
    }
}
