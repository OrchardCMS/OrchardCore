using System.Diagnostics;
using OrchardCore.DataPipelines;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineStateTests
{
    [Fact]
    public async Task Run_IncrementalSource_ReadsOnlyTheItemsChangedSinceTheLastSuccessfulRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "First post");
        var pipelineId = await CreatePipelineAsync(context, "xlsx");

        // Act
        var first = await RunAsync(context, pipelineId);
        var firstRows = await ReadExportAsync(context);

        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Second post");
        var second = await RunAsync(context, pipelineId);
        var secondRows = await ReadExportAsync(context);

        // Assert
        Assert.True(first.Status == DataPipelineRunStatus.Succeeded, first.Error);
        Assert.True(second.Status == DataPipelineRunStatus.Succeeded, second.Error);
        Assert.Contains("First post", firstRows);
        Assert.Equal(["Second post"], secondRows);
    }

    [Fact]
    public async Task StartOver_IncrementalSource_NextRunReadsEveryItemAgain()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "First post");
        var pipelineId = await CreatePipelineAsync(context, "xlsx");
        await RunAsync(context, pipelineId);

        var hadState = false;

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var stateManager = scope.ServiceProvider.GetRequiredService<DataPipelineStateManager>();
            hadState = await stateManager.HasStateAsync(pipelineId);
            await stateManager.ClearAsync(pipelineId);
        });

        var run = await RunAsync(context, pipelineId);
        var rows = await ReadExportAsync(context);

        // Assert
        Assert.True(hadState);
        Assert.True(run.Status == DataPipelineRunStatus.Succeeded, run.Error);
        Assert.Contains("First post", rows);
    }

    [Fact]
    public async Task Run_Failed_KeepsTheStateOfTheLastSuccessfulRun()
    {
        // Arrange: the media library rejects CSV files by default, so the run fails after the source read its rows.
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "First post");
        var pipelineId = await CreatePipelineAsync(context, "csv");

        // Act
        var run = await RunAsync(context, pipelineId);
        var hasState = true;

        await context.UsingTenantScopeAsync(async scope =>
        {
            hasState = await scope.ServiceProvider.GetRequiredService<DataPipelineStateManager>().HasStateAsync(pipelineId);
        });

        // Assert
        Assert.Equal(DataPipelineRunStatus.Failed, run.Status);
        Assert.False(hasState);
    }

    [Fact]
    public async Task HasActiveRun_QueuedRun_IsTrueUntilTheRunEnds()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, "xlsx");
        string runId = null;
        var whileQueued = false;
        var afterRun = true;

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var runManager = scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>();
            runId = (await runManager.QueueAsync(pipeline, new DataPipelineRunRequest())).RunId;
            whileQueued = await runManager.HasActiveRunAsync(pipelineId);
        });

        await WaitForRunAsync(context, runId);

        await context.UsingTenantScopeAsync(async scope =>
        {
            afterRun = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().HasActiveRunAsync(pipelineId);
        });

        // Assert
        Assert.True(whileQueued);
        Assert.False(afterRun);
    }

    [Fact]
    public async Task Delete_PipelineWithState_DeletesTheState()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "First post");
        var pipelineId = await CreatePipelineAsync(context, "xlsx");
        await RunAsync(context, pipelineId);

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            await manager.DeleteAsync(await manager.GetAsync(pipelineId));
        });

        var hasState = true;

        await context.UsingTenantScopeAsync(async scope =>
        {
            hasState = await scope.ServiceProvider.GetRequiredService<DataPipelineStateManager>().HasStateAsync(pipelineId);
        });

        // Assert
        Assert.False(hasState);
    }

    [Fact]
    public void State_SetAndRemove_TrackChanges()
    {
        // Arrange
        var state = new DataPipelineState(new Dictionary<string, System.Text.Json.Nodes.JsonNode> { ["a"] = 1 });

        // Act & Assert
        Assert.False(state.IsChanged);
        Assert.Equal(1, state.Get<int>("a"));

        state.Remove("missing");
        Assert.False(state.IsChanged);

        state.Set("b", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        Assert.True(state.IsChanged);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), state.Get<DateTime>("b"));

        state.Set<string>("a", null);
        Assert.False(state.TryGet<int>("a", out _));
        Assert.Equal(["b"], state.Keys);
    }

    private static async Task<string> CreatePipelineAsync(BlogContext context, string format)
    {
        string pipelineId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Changed posts", null, admin);
            pipelineId = pipeline.PipelineId;

            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft =>
            {
                draft.Steps =
                [
                    Step("source", DataSourceStep.StepName, new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost", Fields = ["DisplayText"], IncrementalField = "ModifiedUtc" }),
                    Step("file", CreateFileStep.StepName, new CreateFileStepSettings { Format = format, FileName = "changed" }),
                    Step("media", SaveToMediaStep.StepName, new SaveToMediaStepSettings { Folder = "exports" }),
                ];
                draft.Connections = [Connect("source", "file"), Connect("file", "media")];
            }, admin);

            await manager.PublishAsync(pipeline, admin);
        });

        return pipelineId;
    }

    private static async Task<DataPipelineRun> RunAsync(BlogContext context, string pipelineId)
    {
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            runId = (await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().QueueAsync(pipeline, new DataPipelineRunRequest())).RunId;
        });

        return await WaitForRunAsync(context, runId);
    }

    private static async Task<List<string>> ReadExportAsync(BlogContext context)
    {
        var rows = new List<string>();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();
            await using var stream = await store.GetFileStreamAsync("exports/changed.xlsx");
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy);
            copy.Position = 0;

            var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
            var (_, read) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(copy, new DataFileOptions()));
            rows.AddRange(read.Select(row => (string)row[0]));
        });

        return rows;
    }

    private static DataPipelineStep Step<TSettings>(string id, string type, TSettings settings)
        where TSettings : class, new()
    {
        var step = new DataPipelineStep { StepId = id, Type = type };
        step.Put(settings);

        return step;
    }

    private static DataPipelineConnection Connect(string source, string target)
    {
        return new() { SourceStepId = source, SourcePort = DataPipelinePort.Output, TargetStepId = target, TargetPort = DataPipelinePort.Input };
    }

    private static async Task<DataPipelineRun> WaitForRunAsync(BlogContext context, string runId)
    {
        var stopwatch = Stopwatch.StartNew();
        DataPipelineRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                run = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().GetAsync(runId);
            });

            if (run?.IsCompleted == true)
            {
                return run;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"The run didn't end; its status is {run?.Status}.");
    }

    private static async Task<ClaimsPrincipal> GetAdminAsync(ShellScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IUser>>();

        return await factory.CreateAsync(await userManager.FindByNameAsync("admin"));
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id is "OrchardCore.DataPipelines" or "OrchardCore.Media")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });
        await DataPipelineTestSites.WaitForFeaturesAsync(context);

        return context;
    }
}
