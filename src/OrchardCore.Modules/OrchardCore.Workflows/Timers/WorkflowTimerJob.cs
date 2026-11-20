using OrchardCore.Jobs;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Timers;

/// <summary>
/// Triggers the timer event of the workflows: starts the workflows that begin with a timer, and resumes the ones
/// waiting on a timer that is due. It runs every minute, once whatever the number of servers, so a timer never
/// fires on two servers at once.
/// </summary>
public sealed class WorkflowTimerJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "WorkflowTimers";

    private readonly IWorkflowManager _workflowManager;

    public WorkflowTimerJob(IWorkflowManager workflowManager)
    {
        _workflowManager = workflowManager;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
        => _workflowManager.TriggerEventAsync(TimerEvent.EventName, null, null, isExclusive: true, isAlwaysCorrelated: true);
}
