using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Recipes;
using OrchardCore.Recipes.Models;
using OrchardCore.Deployment.Steps;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentPlanServiceTests
{
    [Fact]
    public async Task TenantScopedOperations_CannotReadOrMutateAnotherTenantsPlan()
    {
        using var owner = await CreateContextAsync();
        using var other = await CreateContextAsync();
        long id = 0;
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            id = (await plans.CreateAsync("Owner only")).Plan.Id;
            await plans.AddStepsAsync(id, [new RecipeFileDeploymentStep { Id = "original" }]);
        });
        await other.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            Assert.Null(await plans.GetAsync(id));
            Assert.Null(await plans.FindByNameAsync("Owner only"));
            Assert.Empty((await plans.ListAsync("Owner only", 0, 20)).Items);
            Assert.Equal(DeploymentPlanManagementError.NotFound, (await plans.RenameAsync(id, "Foreign edit")).Error);
            Assert.False(await plans.DeleteAsync(id));
            Assert.Equal(DeploymentStepManagementError.NotFound,
                (await plans.AddStepsAsync(id, [new RecipeFileDeploymentStep { Id = "foreign" }])).Error);
            Assert.Equal(DeploymentStepManagementError.NotFound,
                (await plans.UpdateStepAsync(id, new RecipeFileDeploymentStep { Id = "original" })).Error);
            Assert.Equal(DeploymentStepManagementError.NotFound, (await plans.DeleteStepAsync(id, "original")).Error);
            Assert.Equal(DeploymentStepManagementError.NotFound, (await plans.ReorderStepsAsync(id, ["original"])).Error);
        });
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var plan = await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetAsync(id);
            Assert.Equal("Owner only", plan.Name);
            Assert.Equal("original", Assert.Single(plan.DeploymentSteps).Id);
        });
    }

    [Fact]
    public async Task Migration_RepairsLegacyIds_PreservesDisabledStepPayloadAndIsRepeatable()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var unknown = new UnknownDeploymentStep
            {
                Name = "Disabled", TypeDiscriminator = "DisabledStep",
            };
            unknown.RawData = JsonDocument.Parse("""
                {"$type":"DisabledStep","Name":"Disabled","Nested":{"Values":[1,2,3]},"Enabled":false}
                """).RootElement.Clone();
            await scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>().SaveAsync(new DeploymentPlan
            {
                Name = "Legacy",
                DeploymentSteps = [new RecipeFileDeploymentStep { Id = "stable" },
                    new CustomFileDeploymentStep { Id = "STABLE", FileName = "readme.txt", FileContent = "preserve" }, unknown],
            });
        });
        string[] ids = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var migration = ActivatorUtilities.CreateInstance<global::OrchardCore.Deployment.Migrations>(scope.ServiceProvider);
            Assert.Equal(2, await migration.UpdateFrom1Async());
            var plan = Assert.Single(await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetDeploymentPlansAsync("Legacy"));
            ids = plan.DeploymentSteps.Select(step => step.Id).ToArray();
            Assert.Equal("stable", ids[0]);
            Assert.All(ids, id => Assert.False(string.IsNullOrWhiteSpace(id)));
            Assert.Equal(3, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var migration = ActivatorUtilities.CreateInstance<global::OrchardCore.Deployment.Migrations>(scope.ServiceProvider);
            Assert.Equal(2, await migration.UpdateFrom1Async());
            var plan = Assert.Single(await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetDeploymentPlansAsync("Legacy"));
            Assert.Equal(ids, plan.DeploymentSteps.Select(step => step.Id));
            Assert.Equal("preserve", Assert.IsType<CustomFileDeploymentStep>(plan.DeploymentSteps[1]).FileContent);
            var unknown = Assert.IsType<UnknownDeploymentStep>(plan.DeploymentSteps[2]);
            Assert.Equal("DisabledStep", unknown.TypeDiscriminator);
            Assert.Equal(ids[2], unknown.RawData.GetProperty("Id").GetString());
            Assert.Equal("[1,2,3]", unknown.RawData.GetProperty("Nested").GetProperty("Values").GetRawText());
            Assert.False(unknown.RawData.GetProperty("Enabled").GetBoolean());
        });
    }

    [Fact]
    public async Task Recipe_WithoutIds_ReusesSameSlotIdentitiesOnReplay()
    {
        using var context = await CreateContextAsync();
        string[] ids = null;
        for (var run = 0; run < 2; run++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var recipe = new RecipeExecutionContext
                {
                    Name = "deployment",
                    Step = JsonNode.Parse("""
                        {"Plans":[{"Name":"Replay","Steps":[{"Type":"RecipeFileDeploymentStep","Step":{}},
                          {"Type":"CustomFileDeploymentStep","Step":{"FileName":"readme.txt","FileContent":"sample"}}]}]}
                        """).AsObject(),
                };
                await ActivatorUtilities.CreateInstance<DeploymentPlansRecipeStep>(scope.ServiceProvider).ExecuteAsync(recipe);
                Assert.Empty(recipe.Errors);
                var plan = Assert.Single(await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetDeploymentPlansAsync("Replay"));
                var current = plan.DeploymentSteps.Select(step => step.Id).ToArray();
                Assert.All(current, id => Assert.False(string.IsNullOrWhiteSpace(id)));
                Assert.Equal(2, current.Distinct().Count());
                if (ids is not null) { Assert.Equal(ids, current); }
                ids = current;
            });
        }
    }

    [Theory]
    [InlineData("{\"Plans\":{}}")]
    [InlineData("{\"Plans\":[null]}")]
    [InlineData("{\"Plans\":[{\"Name\":\" \"}]}")]
    [InlineData("{\"Plans\":[{\"Name\":\"Keep\",\"Steps\":[{\"Type\":\"MissingFactory\",\"Step\":{}}]}]}")]
    [InlineData("{\"Plans\":[{\"Name\":\"Keep\",\"Steps\":[{\"Type\":\"CustomFileDeploymentStep\",\"Step\":{\"FileName\":\"../escape.txt\"}}]}]}")]
    [InlineData("{\"Plans\":[{\"Name\":\"Keep\"},{\"Name\":\"keep\"}]}")]
    [InlineData("{\"Plans\":[{\"Name\":\"Keep\",\"Steps\":[{\"Type\":\"RecipeFileDeploymentStep\",\"Step\":{\"Id\":\"same\"}},{\"Type\":\"RecipeFileDeploymentStep\",\"Step\":{\"Id\":\"SAME\"}}]}]}")]
    public async Task Recipe_InvalidBatch_ReportsErrorsAndPreservesPersistedPlan(string json)
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().CreateOrUpdateDeploymentPlansAsync(
                [new DeploymentPlan { Name = "Keep", DeploymentSteps = [new RecipeFileDeploymentStep { Id = "original" }] }]));
        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipe = new RecipeExecutionContext { Name = "deployment", Step = JsonNode.Parse(json).AsObject() };
            await ActivatorUtilities.CreateInstance<DeploymentPlansRecipeStep>(scope.ServiceProvider).ExecuteAsync(recipe);
            Assert.NotEmpty(recipe.Errors);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var plan = Assert.Single(await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetDeploymentPlansAsync("Keep"));
            Assert.Equal("original", Assert.Single(plan.DeploymentSteps).Id);
        });
    }

    [Fact]
    public async Task Recipe_ValidReplacement_PersistsConfigurationAndCanonicalFactoryName()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipe = new RecipeExecutionContext
            {
                Name = "deployment",
                Step = JsonNode.Parse("""
                    {"Plans":[{"Name":"Recipe plan","Steps":[{"Type":"CustomFileDeploymentStep",
                      "Step":{"Id":"file","Name":"incorrect","FileName":"folder/readme.txt","FileContent":"sample"}}]}]}
                    """).AsObject(),
            };
            await ActivatorUtilities.CreateInstance<DeploymentPlansRecipeStep>(scope.ServiceProvider).ExecuteAsync(recipe);
            Assert.Empty(recipe.Errors);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var plan = Assert.Single(await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetDeploymentPlansAsync("Recipe plan"));
            var step = Assert.IsType<CustomFileDeploymentStep>(Assert.Single(plan.DeploymentSteps));
            Assert.Equal("file", step.Id);
            Assert.Equal(new CustomFileDeploymentStep().Name, step.Name);
            Assert.Equal("folder/readme.txt", step.FileName);
            Assert.Equal("sample", step.FileContent);
        });
    }

    [Fact]
    public async Task Replace_InvalidLaterPlan_RejectsBatchBeforeChangingExistingPlan()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            await service.CreateOrUpdateDeploymentPlansAsync([new DeploymentPlan
            {
                Name = "Keep", DeploymentSteps = [new RecipeFileDeploymentStep { Id = "original" }],
            }]);
            var existing = Assert.Single(await service.GetDeploymentPlansAsync("Keep"));
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrUpdateDeploymentPlansAsync(
            [
                new DeploymentPlan { Name = "Keep", DeploymentSteps = [new RecipeFileDeploymentStep { Id = "replacement" }] },
                new DeploymentPlan { Name = " " },
            ]));
            Assert.Equal("original", Assert.Single(existing.DeploymentSteps).Id);
        });
    }

    [Fact]
    public async Task Create_AfterCachedRead_IsVisibleToExistingExportCallers()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            await service.GetAllDeploymentPlanNamesAsync();
            await service.CreateOrUpdateDeploymentPlansAsync([new DeploymentPlan { Name = "New export" }]);

            Assert.Contains("New export", await service.GetAllDeploymentPlanNamesAsync());
            Assert.Single(await service.GetDeploymentPlansAsync("New export"));
        });
    }

    [Fact]
    public async Task Replace_WithPreviouslyReadPlan_PreservesItsSteps()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>()
                .CreateOrUpdateDeploymentPlansAsync([new DeploymentPlan
                {
                    Name = "Existing export",
                    DeploymentSteps = [new RecipeFileDeploymentStep { Id = "recipe-file" }],
                }]));

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = Assert.Single(await service.GetDeploymentPlansAsync("Existing export"));
            await service.CreateOrUpdateDeploymentPlansAsync([plan]);
            Assert.Equal("recipe-file", Assert.Single(plan.DeploymentSteps).Id);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = Assert.Single(await service.GetDeploymentPlansAsync("Existing export"));
            Assert.Equal("recipe-file", Assert.Single(plan.DeploymentSteps).Id);
        });
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Deployment");
            await manager.EnableFeaturesAsync([feature], force: true);
        });
        return context;
    }
}
