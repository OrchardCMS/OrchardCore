using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Lists entries for the designer's activities pane that add a registered activity with preset properties (see
/// <see cref="ActivityPreset"/>). The presets can come from code, configuration or data: stored workflows keep
/// referring to the registered activity.
/// </summary>
public interface IActivityPresetProvider
{
    /// <summary>
    /// Returns the presets.
    /// </summary>
    Task<IEnumerable<ActivityPreset>> GetPresetsAsync();
}
