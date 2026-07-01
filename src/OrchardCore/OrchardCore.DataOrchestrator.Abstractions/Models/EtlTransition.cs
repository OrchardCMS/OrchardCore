namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Represents a transition between two activities in an ETL pipeline, analogous to Transition.
/// </summary>
public sealed class EtlTransition
{
    /// <summary>
    /// Gets or sets the source activity identifier.
    /// </summary>
    public string SourceActivityId { get; set; }

    /// <summary>
    /// Gets or sets the outcome name on the source activity.
    /// </summary>
    public string SourceOutcomeName { get; set; }

    /// <summary>
    /// Gets or sets the destination activity identifier.
    /// </summary>
    public string DestinationActivityId { get; set; }
}
