namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// The keys of the data a job run queued by a workflow keeps about it, in its properties.
/// </summary>
public static class JobWorkflowKeys
{
    /// <summary>
    /// The workflow instance that queued the run.
    /// </summary>
    public const string WorkflowId = "WorkflowId";

    /// <summary>
    /// The activity that queued the run.
    /// </summary>
    public const string ActivityId = "WorkflowActivityId";

    /// <summary>
    /// Whether the workflow waits for the run to end.
    /// </summary>
    public const string WaitForCompletion = "WorkflowWaitsForCompletion";
}
