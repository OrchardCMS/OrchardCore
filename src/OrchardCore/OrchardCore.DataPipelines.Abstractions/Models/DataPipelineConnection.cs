namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// Connects an output port of one step to an input port of another, so the data the first step produces flows into
/// the second.
/// </summary>
public sealed class DataPipelineConnection
{
    /// <summary>
    /// Gets or sets the identifier of the step the data comes from.
    /// </summary>
    public string SourceStepId { get; set; }

    /// <summary>
    /// Gets or sets the name of the output port the data comes from.
    /// </summary>
    public string SourcePort { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the step the data goes to.
    /// </summary>
    public string TargetStepId { get; set; }

    /// <summary>
    /// Gets or sets the name of the input port the data goes to.
    /// </summary>
    public string TargetPort { get; set; }
}
