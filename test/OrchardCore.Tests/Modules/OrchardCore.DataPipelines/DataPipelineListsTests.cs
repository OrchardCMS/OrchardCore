using AngleSharp.Html.Parser;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineListsTests
{
    [Fact]
    public async Task ListPipelines_Search_ReturnsTheMatchingPipelinesAndTheirCount()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();

            foreach (var name in new[] { "Orders export", "Customers export", "Orders cleanup" })
            {
                await manager.CreateAsync(name, null, user: null);
            }
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();

            // Act
            var (orders, ordersCount) = await manager.ListAsync("orders", 0, 10);
            var (firstPage, allCount) = await manager.ListAsync(null, 0, 2);

            // Assert
            Assert.Equal(["Orders cleanup", "Orders export"], orders.Select(pipeline => pipeline.Name));
            Assert.Equal(2, ordersCount);
            Assert.Equal(["Customers export", "Orders cleanup"], firstPage.Select(pipeline => pipeline.Name));
            Assert.Equal(3, allCount);
        });
    }

    [Fact]
    public async Task ListRuns_StatusAndSearch_ReturnTheMatchingRuns()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var orders = await manager.CreateAsync("Orders export", null, user: null);
            var customers = await manager.CreateAsync("Customers export", null, user: null);

            await session.SaveAsync(Run("orders-failed", orders, DataPipelineRunStatus.Failed, now.AddMinutes(-3)));
            await session.SaveAsync(Run("orders-succeeded", orders, DataPipelineRunStatus.Succeeded, now.AddMinutes(-2)));
            await session.SaveAsync(Run("customers-succeeded", customers, DataPipelineRunStatus.Succeeded, now.AddMinutes(-1)));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineRunManager>();

            // Act
            var (succeeded, succeededCount) = await manager.ListAsync(new DataPipelineRunFilter { Status = DataPipelineRunStatus.Succeeded }, 0, 10);
            var (orders, _) = await manager.ListAsync(new DataPipelineRunFilter { Search = "orders" }, 0, 10);
            var (none, noneCount) = await manager.ListAsync(new DataPipelineRunFilter { Search = "invoices" }, 0, 10);

            // Assert
            Assert.Equal(["customers-succeeded", "orders-succeeded"], succeeded.Select(run => run.RunId));
            Assert.Equal(2, succeededCount);
            Assert.Equal(["orders-succeeded", "orders-failed"], orders.Select(run => run.RunId));
            Assert.Empty(none);
            Assert.Equal(0, noneCount);
        });
    }

    [Fact]
    public async Task ListSharedFiles_Search_MatchesTheFileOrThePipelineName()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineSharedFileManager>();
            var orders = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().CreateAsync("Orders export", null, user: null);

            await manager.SaveAsync(new DataPipelineSharedFile { FileId = "report", FileName = "report.xlsx", PipelineId = "other", CreatedUtc = now.AddMinutes(-2), ExpiresUtc = now.AddDays(1) });
            await manager.SaveAsync(new DataPipelineSharedFile { FileId = "orders", FileName = "data.csv", PipelineId = orders.PipelineId, PipelineName = orders.Name, CreatedUtc = now.AddMinutes(-1), ExpiresUtc = now.AddDays(1) });
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineSharedFileManager>();

            // Act
            var (byFileName, _) = await manager.ListAsync("REPORT", 0, 10);
            var (byPipelineName, _) = await manager.ListAsync("orders", 0, 10);
            var (all, count) = await manager.ListAsync(null, 0, 10);

            // Assert
            Assert.Equal(["report"], byFileName.Select(file => file.FileId));
            Assert.Equal(["orders"], byPipelineName.Select(file => file.FileId));
            Assert.Equal(["orders", "report"], all.Select(file => file.FileId));
            Assert.Equal(2, count);
        });
    }

    [Fact]
    public async Task PipelinesPage_Search_ListsTheMatchingPipelinesWithTheirActions()
    {
        // Arrange
        using var context = await CreateContextAsync();
        string ordersId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            ordersId = (await manager.CreateAsync("Orders export", null, user: null)).PipelineId;
            await manager.CreateAsync("Customers export", null, user: null);
        });

        // Act
        using var response = await context.Client.GetAsync("Admin/DataPipelines?q=orders", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        using var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var item = Assert.Single(document.QuerySelectorAll("li[data-pipeline-id]"));
        Assert.Equal(ordersId, item.GetAttribute("data-pipeline-id"));
        Assert.Equal("orders", document.QuerySelector("input[name=q]")?.GetAttribute("value"));

        // The secondary actions are in the Actions menu, as in the other lists of the admin.
        var actions = item.QuerySelectorAll(".dropdown-menu .dropdown-item").Select(action => action.TextContent.Trim()).ToList();
        Assert.Contains("Run history", actions);
        Assert.Contains("Delete", actions);
    }

    private static DataPipelineRun Run(string runId, DataPipeline pipeline, DataPipelineRunStatus status, DateTime queuedUtc)
        => new()
        {
            RunId = runId,
            PipelineId = pipeline.PipelineId,
            PipelineName = pipeline.Name,
            Status = status,
            QueuedUtc = queuedUtc,
            CompletedUtc = status is DataPipelineRunStatus.Queued or DataPipelineRunStatus.Running ? null : queuedUtc,
        };

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
