using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using OrchardCore.Workflows.Jobs;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.BackgroundTasks;

/// <summary>
/// Finds the workflow instances whose task faulted and whose retry is due, and queues a run of
/// <see cref="WorkflowRetryJob"/> for each of them. Every server looks for them, which only reads the database; the
/// runs are what retries, and a run is executed by a single server.
/// </summary>
[BackgroundTask(
    Title = "Workflow Retries",
    Schedule = "* * * * *",
    Description = "Queues the retry of the workflow tasks that faulted, when their retry policy's delay is over.")]
public sealed class WorkflowRetryBackgroundTask : IBackgroundTask
{
    // The most instances queued in a run of the task; the others are queued in the next ones.
    private const int BatchSize = 50;

    // After a retry couldn't run, the instance is tried again after this delay, not every minute.
    private static readonly TimeSpan _retryAfter = TimeSpan.FromMinutes(15);

    private readonly IClock _clock;

    public WorkflowRetryBackgroundTask(IClock clock)
    {
        _clock = clock;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var workflowStore = serviceProvider.GetRequiredService<IWorkflowStore>();
        var jobManager = serviceProvider.GetRequiredService<IJobManager>();
        var now = _clock.UtcNow;

        foreach (var workflow in await workflowStore.ListDueRetriesAsync(now, BatchSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var last = await jobManager.FindLastAsync(WorkflowRetryJob.Name, workflow.WorkflowId);

            if (last is not null && (!last.IsCompleted || (last.Status == JobRunStatus.Failed && last.CompletedUtc > now - _retryAfter)))
            {
                continue;
            }

            // The same key makes the servers that find the instance at the same time queue a single run, and makes
            // two runs for the same instance execute one after the other.
            await jobManager.EnqueueAsync<WorkflowRetryJob>(new WorkflowRetryJobParameters { WorkflowId = workflow.WorkflowId }, request =>
            {
                request.Trigger = JobTriggers.Schedule;
                request.DeduplicationKey = workflow.WorkflowId;
                request.ConcurrencyKey = $"{WorkflowRetryJob.Name}:{workflow.WorkflowId}";
                request.CorrelationId = workflow.WorkflowId;
            });
        }
    }
}
