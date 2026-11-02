namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="LimitStep"/>.
/// </summary>
public sealed class LimitStepSettings
{
    /// <summary>
    /// Gets or sets the number of rows to keep.
    /// </summary>
    public int Count { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of rows to skip first.
    /// </summary>
    public int Skip { get; set; }
}
