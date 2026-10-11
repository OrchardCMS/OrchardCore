using System.Text.Json.Nodes;
using OrchardCore.DataPipelines.Deployment;
using OrchardCore.DataPipelines.Ftp;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Recipes;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Stubs;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineRecipeTests
{
    [Fact]
    public async Task Export_PublishedPipeline_ExportsItsDefinitionWithoutSecrets()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);

        // Act
        var step = await ExportAsync(context);

        // Assert
        Assert.Equal(DataPipelinesRecipeStep.RecipeStepName, step["name"]?.GetValue<string>());

        var pipeline = Assert.Single(step["Pipelines"].AsArray()).AsObject();
        Assert.Equal(pipelineId, pipeline["PipelineId"]?.GetValue<string>());
        Assert.Equal("Nightly export", pipeline["Name"]?.GetValue<string>());
        Assert.Equal(2, pipeline["Definition"]["Steps"].AsArray().Count);
        Assert.Single(pipeline["Definition"]["Connections"].AsArray());

        // The secrets are encrypted with the keys of the site: they can't be read elsewhere.
        Assert.Contains("ftp.example.com", step.ToJsonString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Protected", step.ToJsonString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Import_ExportedPipeline_CreatesItAsADraft()
    {
        // Arrange
        using var source = await CreateContextAsync();
        await CreatePublishedPipelineAsync(source);
        var step = await ExportAsync(source);

        using var target = await CreateContextAsync();

        // Act
        await ImportAsync(target, step);

        // Assert
        await target.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = Assert.Single(await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().ListAsync());

            Assert.Equal("Nightly export", pipeline.Name);
            Assert.Equal("Exports the posts.", pipeline.Description);

            // An imported pipeline runs once someone publishes it, with their access.
            Assert.Null(pipeline.Published);
            Assert.Equal(["source", "ftp"], pipeline.Draft.Steps.Select(item => item.StepId));

            var ftp = pipeline.Draft.FindStep("ftp").GetOrCreate<UploadToFtpStepSettings>();
            Assert.Equal("ftp.example.com", ftp.Host);
            Assert.Null(ftp.ProtectedPassword);
        });
    }

    [Fact]
    public async Task Import_ExistingPipeline_ReplacesItsDraftAndKeepsItsPublishedVersion()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePublishedPipelineAsync(context);

        var step = new JsonObject
        {
            ["name"] = DataPipelinesRecipeStep.RecipeStepName,
            ["Pipelines"] = new JsonArray(new JsonObject
            {
                ["PipelineId"] = pipelineId,
                ["Name"] = "Renamed export",
                ["IsEnabled"] = false,
                ["Definition"] = new JsonObject
                {
                    ["Steps"] = new JsonArray(new JsonObject { ["StepId"] = "only", ["Type"] = DataSourceStep.StepName }),
                },
            }),
        };

        // Act
        await ImportAsync(context, step);

        // Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = Assert.Single(await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().ListAsync());

            Assert.Equal(pipelineId, pipeline.PipelineId);
            Assert.Equal("Renamed export", pipeline.Name);
            Assert.False(pipeline.IsEnabled);
            Assert.Equal(["only"], pipeline.Draft.Steps.Select(item => item.StepId));
            Assert.Equal(2, pipeline.Published.Steps.Count);
        });
    }

    [Fact]
    public async Task Import_PipelineWithoutName_ReportsAnError()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new JsonObject
        {
            ["name"] = DataPipelinesRecipeStep.RecipeStepName,
            ["Pipelines"] = new JsonArray(new JsonObject { ["PipelineId"] = "nameless" }),
        };

        // Act
        var errors = await ImportAsync(context, step);

        // Assert
        Assert.Single(errors);
    }

    private static async Task<JsonObject> ExportAsync(BlogContext context)
    {
        JsonObject step = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var fileBuilder = new MemoryFileBuilder();
            var result = new DeploymentPlanResult(fileBuilder, new RecipeDescriptor());
            var deploymentSource = ActivatorUtilities.CreateInstance<AllDataPipelinesDeploymentSource>(scope.ServiceProvider);

            await deploymentSource.ProcessDeploymentStepAsync(new AllDataPipelinesDeploymentStep(), result);
            await result.FinalizeAsync();

            step = JsonNode.Parse(fileBuilder.GetFileContents("Recipe.json", Encoding.UTF8))["steps"][0].AsObject();
        });

        return step;
    }

    private static async Task<IList<string>> ImportAsync(BlogContext context, JsonObject step)
    {
        IList<string> errors = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipeContext = new RecipeExecutionContext { Name = step["name"].GetValue<string>(), Step = step.DeepClone().AsObject() };
            await ActivatorUtilities.CreateInstance<DataPipelinesRecipeStep>(scope.ServiceProvider).ExecuteAsync(recipeContext);
            errors = recipeContext.Errors;
        });

        return errors;
    }

    private static async Task<string> CreatePublishedPipelineAsync(BlogContext context)
    {
        string pipelineId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Nightly export", "Exports the posts.", admin);
            pipelineId = pipeline.PipelineId;

            var source = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
            source.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost" });

            var ftp = new DataPipelineStep { StepId = "ftp", Type = UploadToFtpStep.StepName };
            ftp.Put(new UploadToFtpStepSettings
            {
                Host = "ftp.example.com",
                Username = "exports",
                ProtectedPassword = scope.ServiceProvider.GetRequiredService<DataPipelineSecrets>().Protect("s3cret!"),
            });

            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft =>
            {
                draft.Steps = [source, ftp];
                draft.Connections = [new() { SourceStepId = "source", SourcePort = DataPipelinePort.Output, TargetStepId = "ftp", TargetPort = DataPipelinePort.Input }];
            }, admin);
            await manager.PublishAsync(pipeline, admin);
        });

        return pipelineId;
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
                .Where(feature => feature.Id is "OrchardCore.DataPipelines" or "OrchardCore.DataPipelines.Ftp")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }
}
