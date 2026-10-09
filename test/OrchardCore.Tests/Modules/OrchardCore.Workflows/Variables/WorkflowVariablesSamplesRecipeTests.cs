using System.Text.Json.Nodes;
using OrchardCore.Recipes.Services;
using OrchardCore.Security;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class WorkflowVariablesSamplesRecipeTests
{
    [Fact]
    public async Task SamplesRecipe_Run_AddsWorkflowsThatReplyWithTheirVariables()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        // The test imports the recipe as a deployment package; the recipe enables the features it needs.
        await WorkflowDesignerSiteFixture.EnableFeaturesAsync(context, "OrchardCore.Deployment");

        JsonObject recipe = null;

        // The recipe is listed on the Recipes page like the other recipes of the modules.
        await context.UsingTenantScopeAsync(async scope =>
        {
            foreach (var harvester in scope.ServiceProvider.GetServices<IRecipeHarvester>())
            {
                var descriptor = (await harvester.HarvestRecipesAsync()).FirstOrDefault(recipe => recipe.Name == "workflows-variables-samples");

                if (descriptor is not null)
                {
                    Assert.False(descriptor.IsSetupRecipe);

                    await using var stream = descriptor.RecipeFileInfo.CreateReadStream();
                    recipe = (await JsonNode.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken)).AsObject();
                }
            }
        });

        Assert.NotNull(recipe);

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        Assert.Equal("Guest ordered 1 item(s) for 9.5, with a 0% discount.", await InvokeAsync(context, "sampleordertotal", "orderrequested", string.Empty));
        Assert.Equal("Ann ordered 3 item(s) for 28.5, with a 10% discount.", await InvokeAsync(context, "sampleordertotal", "orderrequested", "customer=Ann&quantity=3"));
        Assert.Equal("Hello, Ann!", await InvokeAsync(context, "samplegreetingrequest", "greetrequested", "name=Ann"));

        await context.UsingTenantScopeAsync(async scope =>
        {
            var greeting = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync("samplegreetingformat");

            Assert.True(greeting.IsActivity);
            Assert.Contains(greeting.Variables, variable => variable.Name == "name" && variable.IsInput);
            Assert.Contains(greeting.Variables, variable => variable.Name == "greeting" && variable.IsOutput);
        });
    }

    // Calls the HTTP request event of a workflow, as the URL its editor generates does.
    private static async Task<string> InvokeAsync(SiteContext context, string workflowTypeId, string activityId, string query)
    {
        string token = null;

        await context.UsingTenantScopeAsync(scope =>
        {
            token = scope.ServiceProvider.GetRequiredService<ISecurityTokenService>()
                .CreateToken(new WorkflowPayload(workflowTypeId, activityId), TimeSpan.FromDays(1));

            return Task.CompletedTask;
        });

        var url = "workflows/invoke/" + Uri.EscapeDataString(token) + (string.IsNullOrEmpty(query) ? string.Empty : "?" + query);

        using var response = await context.Client.GetAsync(url, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Trim();
    }
}
