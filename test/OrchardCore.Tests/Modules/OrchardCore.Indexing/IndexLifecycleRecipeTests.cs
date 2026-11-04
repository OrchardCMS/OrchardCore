using System.Text.Json.Nodes;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexLifecycleRecipeTests
{
    [Theory]
    [InlineData("ResetIndex", "selected")]
    [InlineData("ResetIndex", "all")]
    [InlineData("ResetIndex", "none")]
    [InlineData("RebuildIndex", "selected")]
    [InlineData("RebuildIndex", "all")]
    [InlineData("RebuildIndex", "none")]
    public async Task Execute_QueuesSelectedProfilesThroughTrackedRunner(string step, string selection)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var operationIds = new List<string>();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var profiles = new Mock<IIndexProfileManager>();
            profiles.Setup(manager => manager.GetAllAsync()).ReturnsAsync([
                new IndexProfile { Id = "first", Name = "First" },
                new IndexProfile { Id = "second", Name = "Second" },
            ]);
            using var services = new ServiceCollection().AddSingleton(profiles.Object)
                .AddSingleton(scope.ServiceProvider.GetRequiredService<IndexOperationRunner>()).BuildServiceProvider();
            IRecipeStepHandler handler = step == ResetIndexStep.Key ? new ResetIndexStep(services) : new RebuildIndexStep(services);
            var recipe = new RecipeExecutionContext
            {
                Name = step,
                Step = new JsonObject
                {
                    ["IncludeAll"] = selection == "all",
                    ["IndexNames"] = selection == "selected" ? new JsonArray("FIRST", "missing") : new JsonArray(),
                },
            };
            var http = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            var original = http.HttpContext;
            http.HttpContext = new DefaultHttpContext();
            try
            {
                await handler.ExecuteAsync(recipe);

                Assert.Empty(recipe.Errors);
                using var session = scope.ServiceProvider.GetRequiredService<IStore>().CreateSession();
                var operations = (await session.Query<IndexOperation>().ListAsync()).ToList();
                Assert.Equal(selection == "all" ? 2 : selection == "selected" ? 1 : 0, operations.Count);
                Assert.All(operations, operation =>
                {
                    Assert.Equal(IndexOperationState.Pending, operation.State);
                    Assert.Equal(step == ResetIndexStep.Key ? IndexLifecycleAction.Reset : IndexLifecycleAction.Rebuild, operation.Action);
                    operationIds.Add(operation.OperationId);
                });
                if (selection == "selected") { Assert.Equal("first", Assert.Single(operations).IndexId); }
                profiles.Verify(manager => manager.ResetAsync(It.IsAny<IndexProfile>()), Times.Never());
                profiles.Verify(manager => manager.SynchronizeAsync(It.IsAny<IndexProfile>()), Times.Never());
            }
            finally
            {
                http.HttpContext = original;
            }
        });

        // The fake selection IDs deliberately have no stored profiles. The real background
        // runner must process the queued requests and persist NotFound, never completion.
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (operationIds.Count > 0)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var store = scope.ServiceProvider.GetRequiredService<IndexOperationStore>();
                foreach (var id in operationIds.ToArray())
                {
                    var operation = await store.FindAsync(id);
                    if (operation.State is IndexOperationState.Pending or IndexOperationState.Running) { continue; }
                    Assert.Equal(IndexOperationState.Failed, operation.State);
                    Assert.Equal(IndexProcessingStatus.NotFound, operation.Outcome);
                    operationIds.Remove(id);
                }
            });
            Assert.True(operationIds.Count == 0 || DateTime.UtcNow < deadline, "Recipe background operations did not finish.");
            if (operationIds.Count > 0) { await Task.Delay(100, TestContext.Current.CancellationToken); }
        }
    }
}
