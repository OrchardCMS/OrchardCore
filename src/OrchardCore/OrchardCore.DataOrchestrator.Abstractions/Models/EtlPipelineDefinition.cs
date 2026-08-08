using System.Text.Json.Nodes;

namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Represents a persisted ETL pipeline definition, analogous to WorkflowType.
/// </summary>
public sealed class EtlPipelineDefinition
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique string identifier for this pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the pipeline.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a description of the pipeline.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets whether this pipeline is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the cron schedule expression. Null for manual-only pipelines.
    /// </summary>
    public string Schedule { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last run.
    /// </summary>
    public DateTime? LastRunUtc { get; set; }

    /// <summary>
    /// Gets or sets the activity records in this pipeline.
    /// </summary>
    public IList<EtlActivityRecord> Activities { get; set; } = [];

    /// <summary>
    /// Gets or sets the transitions between activities.
    /// </summary>
    public IList<EtlTransition> Transitions { get; set; } = [];

    /// <summary>
    /// Gets or sets the pipeline parameters.
    /// </summary>
    public IList<EtlPipelineParameter> Parameters { get; set; } = [];
}
