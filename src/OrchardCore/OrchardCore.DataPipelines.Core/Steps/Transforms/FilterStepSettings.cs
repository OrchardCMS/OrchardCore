namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="FilterStep"/>.
/// </summary>
public sealed class FilterStepSettings
{
    /// <summary>
    /// Gets or sets the condition the rows must match: a formula that is true or false.
    /// </summary>
    public string Condition { get; set; }
}
