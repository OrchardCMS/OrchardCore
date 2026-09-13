using OrchardCore.Deployment.Steps;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Services;

internal static class DeploymentRecipeMetadata
{
    public static RecipeDescriptor Create(DeploymentPlan deploymentPlan)
    {
        var recipeDescriptor = new RecipeDescriptor();
        var recipeFileDeploymentStep = deploymentPlan.DeploymentSteps.FirstOrDefault(ds => ds.Name == nameof(RecipeFileDeploymentStep)) as RecipeFileDeploymentStep;

        if (recipeFileDeploymentStep != null)
        {
            recipeDescriptor.Name = recipeFileDeploymentStep.RecipeName;
            recipeDescriptor.DisplayName = recipeFileDeploymentStep.DisplayName;
            recipeDescriptor.Description = recipeFileDeploymentStep.Description;
            recipeDescriptor.Author = recipeFileDeploymentStep.Author;
            recipeDescriptor.WebSite = recipeFileDeploymentStep.WebSite;
            recipeDescriptor.Version = recipeFileDeploymentStep.Version;
            recipeDescriptor.IsSetupRecipe = recipeFileDeploymentStep.IsSetupRecipe;
            recipeDescriptor.Categories = (recipeFileDeploymentStep.Categories ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            recipeDescriptor.Tags = (recipeFileDeploymentStep.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        return recipeDescriptor;
    }
}
