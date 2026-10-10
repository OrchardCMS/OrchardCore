using System.Diagnostics;
using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineAdminPagesTests
{
    [Fact]
    public async Task AdminPages_Render_WithTheirBreadcrumbs()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var (pipelineId, runId) = await RunPipelineAsync(context);

        string[] paths =
        [
            "Admin/DataPipelines",
            "Admin/DataPipelines/Create",
            $"Admin/DataPipelines/Runs/{pipelineId}",
            $"Admin/DataPipelines/Run/{runId}",
            "Admin/DataPipelines/SharedFiles",
        ];

        foreach (var path in paths)
        {
            // Act
            using var document = await GetPageAsync(context, path);

            // Assert
            Assert.True(document.QuerySelectorAll("breadcrumb, breadcrumb-item").Length == 0, $"The breadcrumb of {path} isn't rendered.");
            Assert.NotNull(document.QuerySelector("h1.oc-breadcrumb-title"));
        }
    }

    [Fact]
    public async Task RunPage_StepsWithoutTitle_AreNamedByTheirType()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var (_, runId) = await RunPipelineAsync(context);

        // Act
        using var document = await GetPageAsync(context, $"Admin/DataPipelines/Run/{runId}");

        // Assert
        var steps = document.QuerySelectorAll("table tbody tr td:first-child").Select(cell => cell.TextContent.Trim()).ToList();
        Assert.Contains("Read a data source", steps);
        Assert.Contains("Exported posts", steps);
        Assert.DoesNotContain(DataSourceStep.StepName, steps);
    }

    [Fact]
    public async Task DesignerDefinition_PipelineNotPublishedYet_OffersToRunIt()
    {
        // Arrange
        using var context = await CreateContextAsync();
        string pipelineId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().CreateAsync("Draft only", null, await GetAdminAsync(scope));
            pipelineId = pipeline.PipelineId;
        });

        // Act
        using var response = await context.Client.GetAsync($"Admin/DataPipelines/{pipelineId}/Designer/Definition", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        // The designer enables its Run button once the pipeline is published.
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(json.RootElement.GetProperty("canRun").GetBoolean());
    }

    private static async Task<(string PipelineId, string RunId)> RunPipelineAsync(BlogContext context)
    {
        string pipelineId = null;
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Pages", null, admin);
            pipelineId = pipeline.PipelineId;

            var source = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
            source.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost", Fields = ["DisplayText"] });

            var file = new DataPipelineStep { StepId = "file", Type = CreateFileStep.StepName, Title = "Exported posts" };
            file.Put(new CreateFileStepSettings { Format = "csv", FileName = "posts" });

            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft =>
            {
                draft.Steps = [source, file];
                draft.Connections = [new() { SourceStepId = "source", SourcePort = DataPipelinePort.Output, TargetStepId = "file", TargetPort = DataPipelinePort.Input }];
            }, admin);
            await manager.PublishAsync(pipeline, admin);

            var run = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().QueueAsync(pipeline, new DataPipelineRunRequest { TriggeredBy = admin });
            runId = run.RunId;
        });

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            DataPipelineRun run = null;

            await context.UsingTenantScopeAsync(async scope =>
            {
                run = await scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>().GetAsync(runId);
            });

            if (run?.IsCompleted == true)
            {
                Assert.True(run.Status == DataPipelineRunStatus.Succeeded, run.Error);

                return (pipelineId, runId);
            }

            await Task.Delay(250, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("The run didn't end.");
    }

    private static async Task<IDocument> GetPageAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
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
                .Where(feature => feature.Id == "OrchardCore.DataPipelines")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }
}
