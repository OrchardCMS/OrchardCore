using OrchardCore.DataPipelines.Steps;
using OrchardCore.Entities;

namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// A run of a pipeline: when it was queued and ran, who started it, how each step went, what it delivered, and its
/// log. Extensions keep their own data, such as the workflow that waits for the run, in <see cref="Entity.Properties"/>.
/// </summary>
public sealed class DataPipelineRun : Entity
{
    /// <summary>
    /// The number of log entries a run keeps.
    /// </summary>
    public const int MaxLogEntries = 200;

    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the run.
    /// </summary>
    public string RunId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline when the run was queued.
    /// </summary>
    public string PipelineName { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the version that runs.
    /// </summary>
    public string VersionId { get; set; }

    /// <summary>
    /// Gets or sets the number of the version that runs.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the definition that runs.
    /// </summary>
    public DataPipelineDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets how far the run got.
    /// </summary>
    public DataPipelineRunStatus Status { get; set; }

    /// <summary>
    /// Gets or sets what started the run, such as <c>Manual</c> or <c>Workflow</c>.
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who started the run, if any.
    /// </summary>
    public string TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user the run reads and writes data for.
    /// </summary>
    public string RunAsUserId { get; set; }

    /// <summary>
    /// Gets or sets an identifier that links the run to what started it, such as a workflow instance.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the values passed to the run.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets when the run was queued, in UTC.
    /// </summary>
    public DateTime QueuedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the run started, in UTC.
    /// </summary>
    public DateTime? StartedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the run ended, in UTC.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the run last reported its progress, in UTC. A running run that stops reporting is
    /// considered stopped.
    /// </summary>
    public DateTime? HeartbeatUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user asked to cancel the run.
    /// </summary>
    public bool CancellationRequested { get; set; }

    /// <summary>
    /// Gets or sets why the run failed.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the step that failed, if any.
    /// </summary>
    public string FailedStepId { get; set; }

    /// <summary>
    /// Gets or sets how each step went.
    /// </summary>
    public List<DataPipelineStepRun> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets what the run delivered.
    /// </summary>
    public List<DataPipelineDelivery> Deliveries { get; set; } = [];

    /// <summary>
    /// Gets or sets the messages recorded during the run, the oldest first, up to <see cref="MaxLogEntries"/>.
    /// </summary>
    public List<DataPipelineLogEntry> Log { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether the run ended.
    /// </summary>
    public bool IsCompleted => Status is DataPipelineRunStatus.Succeeded or DataPipelineRunStatus.Failed or DataPipelineRunStatus.Cancelled;
}

/// <summary>
/// How one step went during a run.
/// </summary>
public sealed class DataPipelineStepRun
{
    /// <summary>
    /// Gets or sets the identifier of the step.
    /// </summary>
    public string StepId { get; set; }

    /// <summary>
    /// Gets or sets the step type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the title of the step.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets how far the step got.
    /// </summary>
    public DataPipelineStepStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of rows the step read.
    /// </summary>
    public long RowsIn { get; set; }

    /// <summary>
    /// Gets or sets the number of rows the step wrote.
    /// </summary>
    public long RowsOut { get; set; }

    /// <summary>
    /// Gets or sets the number of files the step read.
    /// </summary>
    public long FilesIn { get; set; }

    /// <summary>
    /// Gets or sets the number of files the step wrote.
    /// </summary>
    public long FilesOut { get; set; }

    /// <summary>
    /// Gets or sets the number of warnings the step reported.
    /// </summary>
    public long Warnings { get; set; }

    /// <summary>
    /// Gets or sets why the step failed.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets when the step started, in UTC.
    /// </summary>
    public DateTime? StartedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the step ended, in UTC.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }
}
