namespace OrchardCore.Workflows.Trimming;

public class WorkflowTrimmingOptions
{
    /// <summary>
    /// The number of workflow instances to delete in a single run. Lower this if workflow trimming is timing out, or
    /// increase it if you have a large number of workflow instances to delete and you don't experience any timeouts.
    /// </summary>
    public int BatchSize { get; set; } = 5000;
}
