using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.Jobs;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Timers;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows;

public sealed class WorkflowTimerJobTests
{
    [Fact]
    public async Task Jobs_TimersFeature_DeclareAScheduleEveryMinuteWhoseRunsAreNotKept()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows", "OrchardCore.Workflows.Timers");
        IReadOnlyList<JobSchedule> schedules = [];
        JobDescriptor job = null;

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            schedules = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().ListAsync();
            job = scope.ServiceProvider.GetRequiredService<IOptions<JobOptions>>().Value.Find(WorkflowTimerJob.Name);
        });

        // Assert
        var timers = Assert.Single(schedules, schedule => schedule.JobName == WorkflowTimerJob.Name);
        Assert.Equal("* * * * *", timers.Cron);
        Assert.True(timers.IsEnabled);
        Assert.True(timers.IsDeclared);

        // There is a run per minute.
        Assert.False(job.KeepSucceededRuns);
    }

    [Fact]
    public async Task WorkflowTimerJob_WorkflowStartingWithATimer_StartsItOnce()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows", "OrchardCore.Workflows.Timers");

        var workflowType = new WorkflowType
        {
            WorkflowTypeId = Guid.NewGuid().ToString("n"),
            Name = "Timed workflow",
            IsEnabled = true,
            Activities =
            [
                new ActivityRecord
                {
                    ActivityId = "timer",
                    Name = nameof(TimerEvent),
                    IsStart = true,
                    Properties = new JsonObject { ["CronExpression"] = "0 0 1 1 *" },
                },
            ],
        };

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(workflowType));

        // Act: two runs, as when the timers are evaluated on the following minute.
        await RunAsync(context);
        await RunAsync(context);

        // Assert: the workflow waits on its timer, and the second run didn't start another one.
        IEnumerable<Workflow> workflows = [];
        await context.UsingTenantScopeAsync(async scope => workflows = await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().ListAsync(workflowType.WorkflowTypeId));

        var workflow = Assert.Single(workflows);
        Assert.Equal(WorkflowStatus.Halted, workflow.Status);
    }

    // Runs the timers job and waits for its run to end. A run that succeeded is not kept, a run that failed is.
    private static async Task RunAsync(SiteContext context)
    {
        string runId = null;
        await context.UsingTenantScopeAsync(async scope => runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<WorkflowTimerJob>()).RunId);

        var stopwatch = Stopwatch.StartNew();
        JobRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            await context.UsingTenantScopeAsync(async scope => run = await scope.ServiceProvider.GetRequiredService<IJobManager>().GetAsync(runId));

            // The run is deleted right after it succeeded, so it may still be seen as succeeded.
            if (run is null || run.Status == JobRunStatus.Succeeded)
            {
                return;
            }

            Assert.False(run.IsCompleted, $"The run of the timers ended as {run.Status}: {run.Error}");

            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"The run of the timers didn't end; its status is {run?.Status}.");
    }
}
