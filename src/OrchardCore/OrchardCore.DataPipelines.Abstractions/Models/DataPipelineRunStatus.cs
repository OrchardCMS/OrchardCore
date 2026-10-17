namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// Identifies how far a run of a pipeline got.
/// </summary>
public enum DataPipelineRunStatus
{
    /// <summary>
    /// The run waits for a worker to start it.
    /// </summary>
    Queued,

    /// <summary>
    /// The run is executing.
    /// </summary>
    Running,

    /// <summary>
    /// Every step completed.
    /// </summary>
    Succeeded,

    /// <summary>
    /// A step failed, or the pipeline had errors.
    /// </summary>
    Failed,

    /// <summary>
    /// The run was cancelled.
    /// </summary>
    Cancelled,
}
