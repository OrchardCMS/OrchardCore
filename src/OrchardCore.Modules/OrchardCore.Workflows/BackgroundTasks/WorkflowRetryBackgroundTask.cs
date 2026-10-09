using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.BackgroundTasks;

/// <summary>
/// Runs the due retries of the tasks that faulted and are retried later by their retry policy.
/// </summary>
[BackgroundTask(
    Title = "Workflow Retries",
    Schedule = "* * * * *",
    Description = "Retries the workflow tasks that faulted, when their retry policy's delay is over.")]
public sealed class WorkflowRetryBackgroundTask : IBackgroundTask
{
    // The most instances retried in a run; the others are retried in the next runs.
    private const int BatchSize = 50;

    private readonly IClock _clock;
    private readonly ILogger _logger;

    public WorkflowRetryBackgroundTask(IClock clock, ILogger<WorkflowRetryBackgroundTask> logger)
    {
        _clock = clock;
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var workflowStore = serviceProvider.GetRequiredService<IWorkflowStore>();
        var workflowManager = serviceProvider.GetRequiredService<IWorkflowManager>();

        foreach (var workflow in await workflowStore.ListDueRetriesAsync(_clock.UtcNow, BatchSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await workflowManager.RunDueRetryAsync(workflow);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "The retry of the workflow '{WorkflowId}' failed.", workflow.WorkflowId);
            }
        }
    }
}
