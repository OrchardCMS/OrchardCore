using System.Diagnostics;
using System.Text.Json.Nodes;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.Jobs;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Jobs;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows;

public sealed class JobWorkflowTests
{
    [Fact]
    public async Task RunJobTask_WaitingForTheRun_ResumesTheWorkflowWithTheRun()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        var key = Guid.NewGuid().ToString("n");
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(TestJob.ManualName, key));

        // Act
        var workflowId = await StartWorkflowAsync(context, workflowTypeId, WorkflowStatus.Halted);
        var run = await WaitForResumedRunAsync(context, workflowTypeId, workflowId);

        // Assert
        Assert.Equal(nameof(JobRunStatus.Succeeded), run["Status"]?.GetValue<string>());
        Assert.Equal(TestJob.ManualName, run["JobName"]?.GetValue<string>());
        Assert.Equal("From a workflow", run["Title"]?.GetValue<string>());
        Assert.Equal(1, run["Result"]?["Executions"]?.GetValue<int>());
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task RunJobTask_RunThatFails_ResumesTheWorkflowWithItsError()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        var key = Guid.NewGuid().ToString("n");
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(TestJob.ManualName, key, failCount: 1));

        // Act
        var workflowId = await StartWorkflowAsync(context, workflowTypeId, WorkflowStatus.Halted);
        var run = await WaitForResumedRunAsync(context, workflowTypeId, workflowId);

        // Assert
        Assert.Equal(nameof(JobRunStatus.Failed), run["Status"]?.GetValue<string>());
        Assert.Equal("Test failure 1", run["Error"]?.GetValue<string>());
    }

    [Fact]
    public async Task RunJobTask_JobThatCannotBeRunManually_DoesNotQueueARun()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        var key = Guid.NewGuid().ToString("n");
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(TestJob.Name, key));

        // Act: the task follows its Failed outcome at once, so the workflow ends.
        await StartWorkflowAsync(context, workflowTypeId, WorkflowStatus.Finished);
        await Task.Delay(500, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task JobRunCompletedEvent_RunOfTheJobEnds_StartsTheWorkflow()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        var workflowTypeId = await CreateWorkflowTypeAsync(context, new ActivityRecord
        {
            ActivityId = "completed",
            Name = nameof(JobRunCompletedEvent),
            IsStart = true,
            Properties = new JsonObject { ["JobName"] = TestJob.Name, ["Status"] = nameof(JobRunStatus.Succeeded) },
        });

        // Act: only the run of the job that succeeds starts the workflow.
        string succeededRunId = null;
        string failedRunId = null;
        string otherRunId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
            succeededRunId = (await jobManager.EnqueueAsync(TestJob.Name, new TestJobParameters { Key = Guid.NewGuid().ToString("n") })).RunId;
            failedRunId = (await jobManager.EnqueueAsync(TestJob.Name, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), FailCount = 1 })).RunId;
            otherRunId = (await jobManager.EnqueueAsync(TestJob.ManualName, new TestJobParameters { Key = Guid.NewGuid().ToString("n") })).RunId;
        });

        await JobsTestSite.WaitForRunAsync(context, succeededRunId);
        await JobsTestSite.WaitForRunAsync(context, failedRunId);
        await JobsTestSite.WaitForRunAsync(context, otherRunId);

        // Assert
        var stopwatch = Stopwatch.StartNew();
        var started = 0;

        while (started == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            await Task.Delay(250, TestContext.Current.CancellationToken);
            await context.UsingTenantScopeAsync(async scope => started = await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().CountAsync(workflowTypeId));
        }

        // The handlers of the other runs had the time to run as well.
        await Task.Delay(1000, TestContext.Current.CancellationToken);
        await context.UsingTenantScopeAsync(async scope => started = await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().CountAsync(workflowTypeId));

        Assert.Equal(1, started);
    }

    [Fact]
    public async Task RunWorkflowJob_Workflow_StartsAnInstance()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        var workflowTypeId = await CreateWorkflowTypeAsync(context, new ActivityRecord
        {
            ActivityId = "start",
            Name = nameof(StartedByWorkflowEvent),
            IsStart = true,
            Properties = [],
        });

        // Act
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var run = await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<RunWorkflowJob>(new RunWorkflowJobParameters
            {
                WorkflowTypeId = workflowTypeId,
                CorrelationId = "correlated",
            });

            runId = run.RunId;
        });

        var completed = await JobsTestSite.WaitForRunAsync(context, runId);

        // Assert
        Assert.True(completed.Status == JobRunStatus.Succeeded, completed.Error);
        Assert.Equal(nameof(WorkflowStatus.Finished), completed.Result?["Status"]?.GetValue<string>());
        Assert.False(string.IsNullOrEmpty(completed.Result?["WorkflowId"]?.GetValue<string>()));
    }

    [Fact]
    public async Task RunWorkflowJob_UnknownWorkflow_Fails()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");

        // Act
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<RunWorkflowJob>(new RunWorkflowJobParameters { WorkflowTypeId = "unknown" })).RunId;
        });

        var completed = await JobsTestSite.WaitForRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Failed, completed.Status);
        Assert.Contains("doesn't exist", completed.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Jobs_WorkflowsFeature_DeclareTheTrimmingSchedule()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Workflows");
        IReadOnlyList<JobSchedule> schedules = [];

        // Act
        await context.UsingTenantScopeAsync(async scope => schedules = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().ListAsync());

        // Assert
        var trimming = Assert.Single(schedules, schedule => schedule.JobName == "WorkflowTrimming");
        Assert.Equal("0 0 * * *", trimming.Cron);
        Assert.True(trimming.IsEnabled);
        Assert.True(trimming.IsDeclared);

        // The trimming job runs.
        string runId = null;
        await context.UsingTenantScopeAsync(async scope => runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync("WorkflowTrimming")).RunId);

        var run = await JobsTestSite.WaitForRunAsync(context, runId);
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
    }

    private static ActivityRecord RunActivity(string jobName, string key, int failCount = 0)
    {
        return new()
        {
            ActivityId = "run",
            Name = nameof(RunJobTask),
            IsStart = true,
            Properties = new JsonObject
            {
                ["JobName"] = jobName,
                ["WaitForCompletion"] = true,
                ["RunTitle"] = new JsonObject { ["Expression"] = "From a workflow" },
                ["Parameters"] = new JsonObject { ["Expression"] = $$"""{ "Key": "{{key}}", "FailCount": {{failCount}} }""" },
            },
        };
    }

    private static async Task<string> StartWorkflowAsync(SiteContext context, string workflowTypeId, WorkflowStatus expectedStatus)
    {
        string workflowId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(workflowTypeId);
            var workflowContext = await scope.ServiceProvider.GetRequiredService<IWorkflowManager>().StartWorkflowAsync(workflowType);
            workflowId = workflowContext.WorkflowId;

            Assert.Equal(expectedStatus, workflowContext.Status);
        });

        return workflowId;
    }

    // Waits for the workflow to resume and finish, and returns the run it was resumed with.
    private static async Task<JsonObject> WaitForResumedRunAsync(SiteContext context, string workflowTypeId, string workflowId)
    {
        var stopwatch = Stopwatch.StartNew();
        Workflow workflow = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                // Finding a workflow by its identifier only finds the halted ones.
                workflow = (await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().ListAsync(workflowTypeId))
                    .FirstOrDefault(item => item.WorkflowId == workflowId);
            });

            if (workflow?.Status == WorkflowStatus.Finished)
            {
                return Assert.IsType<JsonObject>(workflow.State["Output"]?[RunJobTask.RunKey]);
            }

            await Task.Delay(250, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"The workflow didn't resume; its status is {workflow?.Status}.");
    }

    private static async Task<string> CreateWorkflowTypeAsync(SiteContext context, ActivityRecord activity)
    {
        var workflowType = new WorkflowType
        {
            WorkflowTypeId = Guid.NewGuid().ToString("n"),
            Name = "Job workflow",
            IsEnabled = true,
            Activities = [activity],
        };

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(workflowType));

        return workflowType.WorkflowTypeId;
    }
}
