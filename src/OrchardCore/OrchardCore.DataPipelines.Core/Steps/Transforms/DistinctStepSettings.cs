namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="DistinctStep"/>.
/// </summary>
public sealed class DistinctStepSettings
{
    /// <summary>
    /// Gets or sets the fields whose values identify a row. When empty, every field does.
    /// </summary>
    public List<string> Fields { get; set; } = [];
}
