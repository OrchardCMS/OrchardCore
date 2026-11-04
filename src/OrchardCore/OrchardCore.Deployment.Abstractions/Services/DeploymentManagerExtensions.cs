using System.Security.Claims;
using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Services;

/// <summary>
/// Provides ways to run a deployment step outside of a stored deployment plan.
/// </summary>
public static class DeploymentManagerExtensions
{
    /// <summary>
    /// Runs one deployment step and returns the resulting recipe, without creating a package.
    /// </summary>
    /// <remarks>
    /// The files a source adds are not part of the recipe, so the step must keep its content inline.
    /// </remarks>
    /// <param name="deploymentManager">The deployment manager.</param>
    /// <param name="step">The step to run.</param>
    /// <param name="recipeName">The name of the recipe.</param>
    /// <param name="user">The principal used to authorize the exported resources.</param>
    /// <returns>The recipe.</returns>
    public static async Task<JsonObject> ExportRecipeAsync(this IDeploymentManager deploymentManager, DeploymentStep step, string recipeName, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(deploymentManager);
        ArgumentNullException.ThrowIfNull(step);

        var plan = new DeploymentPlan { Name = recipeName };
        plan.DeploymentSteps.Add(step);

        var result = new DeploymentPlanResult(new DiscardingFileBuilder(), new RecipeDescriptor { Name = recipeName, DisplayName = recipeName })
        {
            User = user,
        };

        await deploymentManager.ExecuteDeploymentPlanAsync(plan, result);

        return result.Recipe;
    }

    private sealed class DiscardingFileBuilder : IFileBuilder
    {
        public Task SetFileAsync(string subpath, Stream stream) => Task.CompletedTask;
    }
}
