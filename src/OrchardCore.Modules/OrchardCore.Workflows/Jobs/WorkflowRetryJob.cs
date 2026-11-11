using OrchardCore.Jobs;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// The parameters of a run of <see cref="WorkflowRetryJob"/>.
/// </summary>
public sealed class WorkflowRetryJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the workflow instance to retry.
    /// </summary>
    public string WorkflowId { get; set; }
}

/// <summary>
/// Runs the due retry of a workflow instance whose task faulted and is retried later by its retry policy. A run is
/// queued for each such instance, and executed once whatever the number of servers.
/// </summary>
public sealed class WorkflowRetryJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "WorkflowRetry";

    private readonly IWorkflowStore _workflowStore;
    private readonly IWorkflowManager _workflowManager;

    public WorkflowRetryJob(IWorkflowStore workflowStore, IWorkflowManager workflowManager)
    {
        _workflowStore = workflowStore;
        _workflowManager = workflowManager;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<WorkflowRetryJobParameters>();
        var workflow = await _workflowStore.GetAsync(parameters.WorkflowId);

        if (workflow is null)
        {
            context.LogInformation("The workflow instance doesn't exist anymore.");

            return;
        }

        var workflowContext = await _workflowManager.RunDueRetryAsync(workflow);

        if (workflowContext is null)
        {
            context.LogInformation("The workflow instance has no retry that is due anymore, or is being run by another process.");

            return;
        }

        context.LogInformation($"The workflow instance was retried, and is now {workflowContext.Status}.");
        context.SetResult(new { Status = workflowContext.Status.ToString() });
    }
}
