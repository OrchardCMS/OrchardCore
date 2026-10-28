using System.Text.Json.Nodes;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// An entry of the designer's activities pane that adds a registered activity with preset properties, for example a
/// workflow usable as an activity, or a call of an API catalog. See <see cref="Services.IActivityPresetProvider"/>.
/// </summary>
public sealed class ActivityPreset
{
    /// <summary>
    /// The identifier of the preset, unique among all the presets, for example <c>workflow:{id}</c>.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The name of the activity it adds.
    /// </summary>
    public string ActivityName { get; init; }

    /// <summary>
    /// The name shown in the activities pane.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// The category it's listed in, or <see langword="null"/> for the category of its activity.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// What it does, shown in the activities pane.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// The Font Awesome icon class, or <see langword="null"/> for the icon of its activity.
    /// </summary>
    public string Icon { get; init; }

    /// <summary>
    /// The properties set on the activity it adds, over the activity's default properties.
    /// </summary>
    public JsonObject Properties { get; init; } = [];
}
