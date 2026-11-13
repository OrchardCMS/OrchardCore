using System.Text.Json.Nodes;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Timers;

namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// When a job run ends, resumes the workflow that waits for it, and triggers the <see cref="JobRunCompletedEvent"/>.
/// </summary>
public sealed class WorkflowJobRunHandler : IJobRunHandler
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowStore _workflowStore;

    public WorkflowJobRunHandler(IWorkflowManager workflowManager, IWorkflowStore workflowStore)
    {
        _workflowManager = workflowManager;
        _workflowStore = workflowStore;
    }

    public async Task CompletedAsync(JobRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        var input = new Dictionary<string, object> { [RunJobTask.RunKey] = ToDictionary(run) };

        if (run.Properties[JobWorkflowKeys.WaitForCompletion] is JsonValue wait && wait.TryGetValue<bool>(out var waits) && waits &&
            run.Properties[JobWorkflowKeys.WorkflowId]?.GetValue<string>() is { Length: > 0 } workflowId &&
            run.Properties[JobWorkflowKeys.ActivityId]?.GetValue<string>() is { Length: > 0 } activityId)
        {
            var workflow = await _workflowStore.GetAsync(workflowId);
            var blocking = workflow?.BlockingActivities.FirstOrDefault(activity => activity.ActivityId == activityId);

            if (blocking is not null)
            {
                await _workflowManager.ResumeWorkflowAsync(workflow, blocking, input);
            }
        }

        // The runs that only keep the workflows going don't trigger the event: a workflow that follows the runs of
        // every job would otherwise run for each of its own retries, and every minute with the timers.
        if (run.JobName is WorkflowRetryJob.Name or WorkflowTimerJob.Name)
        {
            return;
        }

        await _workflowManager.TriggerEventAsync(nameof(JobRunCompletedEvent), input);
    }

    private static Dictionary<string, object> ToDictionary(JobRun run)
    {
        return new(StringComparer.Ordinal)
        {
            ["RunId"] = run.RunId,
            ["JobName"] = run.JobName,
            ["Title"] = run.Title,
            ["Status"] = run.Status.ToString(),
            ["Error"] = run.Error,
            ["Superseded"] = run.Superseded,
            ["Trigger"] = run.Trigger,
            ["Attempt"] = run.Attempt,
            ["StartedUtc"] = run.StartedUtc,
            ["CompletedUtc"] = run.CompletedUtc,
            ["Result"] = run.Result?.DeepClone(),
        };
    }
}
