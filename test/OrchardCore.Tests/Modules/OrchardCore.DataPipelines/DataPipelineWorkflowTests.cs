using System.Diagnostics;
using System.Text.Json.Nodes;
using OrchardCore.BackgroundTasks;
using OrchardCore.DataPipelines.BackgroundTasks;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataPipelines.Workflows;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineWorkflowTests
{
    [Fact]
    public async Task RunDataPipelineTask_WaitForCompletion_ResumesWhenTheRunSucceeds()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(pipelineId));

        // Act
        var workflowId = await StartWorkflowAsync(context, workflowTypeId);

        // Assert
        var run = await WaitForResumedRunAsync(context, workflowTypeId, workflowId);
        Assert.Equal("Succeeded", run["Status"]?.GetValue<string>());
    }

    [Fact]
    public async Task RunDataPipelineTask_Parameters_ArePassedToTheRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);
        var activity = RunActivity(pipelineId);
        activity.Properties["Parameters"] = new JsonObject { ["Expression"] = "Region=EMEA\nYear={{ 2000 | plus: 26 }}\n\nnot a parameter" };
        var workflowTypeId = await CreateWorkflowTypeAsync(context, activity);

        // Act
        var workflowId = await StartWorkflowAsync(context, workflowTypeId);

        // Assert
        await WaitForResumedRunAsync(context, workflowTypeId, workflowId);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var (runs, _) = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().ListAsync(pipelineId, 0, 10);
            var run = Assert.Single(runs);
            Assert.Equal("EMEA", run.Parameters["Region"]);
            Assert.Equal("2026", run.Parameters["Year"]);
            Assert.Equal(2, run.Parameters.Count);
        });
    }

    [Fact]
    public async Task RunDataPipelineTask_RunCancelledWhileQueued_ResumesTheWorkflow()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(pipelineId));

        // Act: the run is cancelled before the request that queued it ends, so it never starts.
        var workflowId = await StartWorkflowAsync(context, workflowTypeId, async (scope, runId) =>
        {
            var runManager = scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>();
            await runManager.CancelAsync(await runManager.GetAsync(runId), user: null);
        });

        // Assert
        var run = await WaitForResumedRunAsync(context, workflowTypeId, workflowId);
        Assert.Equal("Cancelled", run["Status"]?.GetValue<string>());
    }

    [Fact]
    public async Task RunDataPipelineTask_RunStoppedReporting_ResumesTheWorkflowOnceTheRunIsMarkedFailed()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);
        var workflowTypeId = await CreateWorkflowTypeAsync(context, RunActivity(pipelineId));

        // The run looks like it was running when the site stopped.
        var workflowId = await StartWorkflowAsync(context, workflowTypeId, async (scope, runId) =>
        {
            var run = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().GetAsync(runId);
            run.Status = DataPipelineRunStatus.Running;
            run.HeartbeatUtc = DateTime.UtcNow.AddHours(-1);
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveAsync(run);
        });

        // Act
        await context.UsingTenantScopeAsync(scope => new DataPipelineRunsBackgroundTask().DoWorkAsync(scope.ServiceProvider, TestContext.Current.CancellationToken));

        // Assert
        var run = await WaitForResumedRunAsync(context, workflowTypeId, workflowId);
        Assert.Equal("Failed", run["Status"]?.GetValue<string>());
    }

    [Fact]
    public async Task RunCompletedEvent_RunOfThePipelineEnds_StartsTheWorkflow()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);
        var workflowTypeId = await CreateWorkflowTypeAsync(context, new ActivityRecord
        {
            ActivityId = "completed",
            Name = nameof(DataPipelineRunCompletedEvent),
            IsStart = true,
            Properties = new JsonObject { ["PipelineId"] = pipelineId, ["Status"] = "Succeeded" },
        });

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().QueueAsync(pipeline, new DataPipelineRunRequest());
        });

        // Assert
        var stopwatch = Stopwatch.StartNew();
        var started = 0;

        while (started == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(60))
        {
            await Task.Delay(250, TestContext.Current.CancellationToken);

            await context.UsingTenantScopeAsync(async scope =>
            {
                started = await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().CountAsync(workflowTypeId);
            });
        }

        Assert.Equal(1, started);
    }

    private static ActivityRecord RunActivity(string pipelineId)
        => new()
        {
            ActivityId = "run",
            Name = nameof(RunDataPipelineTask),
            IsStart = true,
            Properties = new JsonObject { ["PipelineId"] = pipelineId, ["WaitForCompletion"] = true },
        };

    // Starts a workflow and returns its identifier. The action runs in the same scope, after the run is queued and
    // before it is dispatched.
    private static async Task<string> StartWorkflowAsync(BlogContext context, string workflowTypeId, Func<ShellScope, string, Task> afterQueued = null)
    {
        string workflowId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(workflowTypeId);
            var workflowContext = await scope.ServiceProvider.GetRequiredService<IWorkflowManager>().StartWorkflowAsync(workflowType);
            workflowId = workflowContext.WorkflowId;

            Assert.Equal(WorkflowStatus.Halted, workflowContext.Status);

            if (afterQueued is not null)
            {
                await afterQueued(scope, (string)workflowContext.LastResult);
            }
        });

        return workflowId;
    }

    // Waits for the workflow to resume and finish, and returns the run it was resumed with.
    private static async Task<JsonObject> WaitForResumedRunAsync(BlogContext context, string workflowTypeId, string workflowId)
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
                return Assert.IsType<JsonObject>(workflow.State["Output"]?[RunDataPipelineTask.RunKey]);
            }

            await Task.Delay(250, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"The workflow didn't resume; its status is {workflow?.Status}.");
    }

    private static async Task<string> CreateWorkflowTypeAsync(BlogContext context, ActivityRecord activity)
    {
        var workflowType = new WorkflowType
        {
            WorkflowTypeId = Guid.NewGuid().ToString("n"),
            Name = "Data pipeline workflow",
            IsEnabled = true,
            Activities = [activity],
        };

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(workflowType));

        return workflowType.WorkflowTypeId;
    }

    private static async Task<string> CreatePublishedPipelineAsync(BlogContext context)
    {
        string pipelineId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var admin = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IUser>>().CreateAsync(await userManager.FindByNameAsync("admin"));
            var pipeline = await manager.CreateAsync("Workflow export", null, admin);
            pipelineId = pipeline.PipelineId;

            var source = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
            source.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost", Fields = ["DisplayText"] });

            var file = new DataPipelineStep { StepId = "file", Type = CreateFileStep.StepName };
            file.Put(new CreateFileStepSettings { Format = "csv", FileName = "posts" });

            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft =>
            {
                draft.Steps = [source, file];
                draft.Connections = [new() { SourceStepId = "source", SourcePort = DataPipelinePort.Output, TargetStepId = "file", TargetPort = DataPipelinePort.Input }];
            }, admin);
            await manager.PublishAsync(pipeline, admin);
        });

        return pipelineId;
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id is "OrchardCore.Workflows" or "OrchardCore.DataPipelines.Workflows")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }
}
