using System.Diagnostics;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources.Files;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineRunTests
{
    [Fact]
    public async Task Run_PublishedPipeline_ExportsContentItemsToTheMediaLibrary()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Exported post");

        string pipelineId = null;
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Daily export", null, admin);
            pipelineId = pipeline.PipelineId;

            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft => Define(draft), admin);
            await manager.PublishAsync(pipeline, admin);

            // Act
            var run = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().QueueAsync(pipeline, new DataPipelineRunRequest { TriggeredBy = admin });
            runId = run.RunId;
        });

        var completed = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(completed.Status == DataPipelineRunStatus.Succeeded, completed.Error);
        Assert.Equal(1, completed.VersionNumber);
        Assert.All(completed.Steps, step => Assert.Equal(DataPipelineStepStatus.Succeeded, step.Status));

        var delivery = Assert.Single(completed.Deliveries);
        Assert.Contains("exports/posts.xlsx", delivery.Url, StringComparison.Ordinal);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();
            await using var stream = await store.GetFileStreamAsync("exports/posts.xlsx");
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy);
            copy.Position = 0;

            var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(copy, new DataFileOptions()));

            Assert.Equal(["DisplayText", "Label"], fields.Select(field => field.Name));
            Assert.Contains(rows, row => (string)row[0] == "Exported post" && (string)row[1] == "EXPORTED POST");
        });
    }

    [Fact]
    public async Task Publish_UnchangedDraft_KeepsTheVersion()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Versions", null, admin);
            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft => Define(draft), admin);

            // Act
            var first = await manager.PublishAsync(pipeline, admin);
            var second = await manager.PublishAsync(pipeline, admin);
            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft => draft.Steps[0].X += 10, admin);
            var third = await manager.PublishAsync(pipeline, admin);

            // Assert
            Assert.Equal(1, first.Number);
            Assert.Equal(first.VersionId, second.VersionId);
            Assert.Equal(2, third.Number);
            Assert.Null(pipeline.Draft);
            Assert.Equal(2, (await manager.ListVersionsAsync(pipeline.PipelineId)).Count);
        });
    }

    [Fact]
    public async Task SaveDraft_StaleRevision_ThrowsConflict()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Conflicts", null, admin);
            var revision = pipeline.Revision;
            await manager.SaveDraftAsync(pipeline, revision, draft => Define(draft), admin);

            // Act & Assert
            var conflict = await Assert.ThrowsAsync<DataPipelineConflictException>(() => manager.SaveDraftAsync(pipeline, revision, draft => draft.Steps.Clear(), admin));
            Assert.Equal(revision + 1, conflict.CurrentRevision);
        });
    }

    [Fact]
    public async Task Queue_PipelineNeverPublished_Throws()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var admin = await GetAdminAsync(scope);
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().CreateAsync("Draft only", null, admin);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().QueueAsync(pipeline, new DataPipelineRunRequest()));
        });
    }

    private static void Define(DataPipelineDefinition draft)
    {
        var source = Step("source", DataSourceStep.StepName, new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost", Fields = ["DisplayText"] });
        var calculate = Step("calculate", CalculatedFieldsStep.StepName, new CalculatedFieldsStepSettings { Fields = [new CalculatedField { Name = "Label", Formula = "UPPER([DisplayText])" }] });
        var file = Step("file", CreateFileStep.StepName, new CreateFileStepSettings { Format = "xlsx", FileName = "posts" });
        var media = Step("media", SaveToMediaStep.StepName, new SaveToMediaStepSettings { Folder = "exports" });

        draft.Steps = [source, calculate, file, media];
        draft.Connections =
        [
            Connect("source", "calculate"),
            Connect("calculate", "file"),
            Connect("file", "media"),
        ];
    }

    private static DataPipelineStep Step<TSettings>(string id, string type, TSettings settings)
        where TSettings : class, new()
    {
        var step = new DataPipelineStep { StepId = id, Type = type };
        step.Put(settings);

        return step;
    }

    private static DataPipelineConnection Connect(string source, string target)
        => new() { SourceStepId = source, SourcePort = DataPipelinePort.Output, TargetStepId = target, TargetPort = DataPipelinePort.Input };

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
                .Where(feature => feature.Id is "OrchardCore.DataPipelines.Media")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }
}
