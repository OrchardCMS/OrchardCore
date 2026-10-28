using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;

public sealed class WorkflowTypeDraftLifecycleTests
{
    [Fact]
    public async Task DeleteAsync_TypeWithDraft_DeletesDraft()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows");

        var workflowTypeId = await CreateTypeWithDraftAsync(context);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>();
            await store.DeleteAsync(await store.GetAsync(workflowTypeId));
        });

        await AssertDraftAsync(context, workflowTypeId, exists: false);
    }

    [Fact]
    public async Task WorkflowTypeRecipeStep_ReplacesTypeWithDraft_DeletesDraft()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows", "OrchardCore.Deployment");

        var workflowTypeId = await CreateTypeWithDraftAsync(context);

        await RecipeHelpers.RunRecipeAsync(context, new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject
            {
                ["name"] = "WorkflowType",
                ["data"] = new JsonArray(new JsonObject
                {
                    ["WorkflowTypeId"] = workflowTypeId,
                    ["Name"] = "Imported",
                    ["IsEnabled"] = true,
                    ["Activities"] = new JsonArray(new JsonObject
                    {
                        ["ActivityId"] = "imported-activity",
                        ["Name"] = "NotifyTask",
                        ["X"] = 10,
                        ["Y"] = 20,
                    }),
                    ["Transitions"] = new JsonArray(),
                }),
            }),
        });

        await AssertDraftAsync(context, workflowTypeId, exists: false);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(workflowTypeId);

            Assert.Equal("Imported", workflowType.Name);
            Assert.Equal("imported-activity", Assert.Single(workflowType.Activities).ActivityId);
        });
    }

    private static async Task<string> CreateTypeWithDraftAsync(SiteContext context)
    {
        var workflowTypeId = Guid.NewGuid().ToString("n");

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(new WorkflowType
            {
                WorkflowTypeId = workflowTypeId,
                Name = "Live",
                IsEnabled = true,
            });
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var result = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>()
                .AddActivityAsync(workflowTypeId, 0, "NotifyTask", 10, 20);

            Assert.True(result.Succeeded);
        });

        await AssertDraftAsync(context, workflowTypeId, exists: true);

        return workflowTypeId;
    }

    private static Task AssertDraftAsync(SiteContext context, string workflowTypeId, bool exists)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var draft = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeDraftManager>().GetAsync(workflowTypeId);

            Assert.Equal(exists, draft is not null);
        });

    private static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await featuresManager.GetAvailableFeaturesAsync();
            var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id)).ToArray();
            Assert.Equal(featureIds.Length, features.Length);
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
    }
}
