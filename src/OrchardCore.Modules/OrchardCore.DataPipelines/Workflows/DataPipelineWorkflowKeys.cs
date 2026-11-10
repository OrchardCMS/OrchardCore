namespace OrchardCore.DataPipelines.Workflows;

/// <summary>
/// The keys of the data a run started by a workflow keeps about it.
/// </summary>
public static class DataPipelineWorkflowKeys
{
    /// <summary>
    /// The workflow instance that started the run.
    /// </summary>
    public const string WorkflowId = "WorkflowId";

    /// <summary>
    /// The activity that started the run.
    /// </summary>
    public const string ActivityId = "WorkflowActivityId";

    /// <summary>
    /// Whether the workflow waits for the run to end.
    /// </summary>
    public const string WaitForCompletion = "WorkflowWaitsForCompletion";
}
