namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Identifies how far a step got during a run.
/// </summary>
public enum DataPipelineStepStatus
{
    /// <summary>
    /// The step hasn't started.
    /// </summary>
    Pending,

    /// <summary>
    /// The step is running.
    /// </summary>
    Running,

    /// <summary>
    /// The step completed.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The step failed, which fails the run.
    /// </summary>
    Failed,

    /// <summary>
    /// The step was stopped because the run was cancelled or another step failed.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The step was not executed, such as a destination during a preview.
    /// </summary>
    Skipped,
}
