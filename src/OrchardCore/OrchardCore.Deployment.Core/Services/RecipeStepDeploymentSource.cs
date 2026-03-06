using OrchardCore.Recipes.Services;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>
/// A deployment source that bridges <see cref="RecipeStepDeploymentStep"/> to
/// <see cref="IRecipeDeploymentStep.ExportAsync"/> for automatic deployment from recipe steps.
/// </summary>
public sealed class RecipeStepDeploymentSource : DeploymentSourceBase<RecipeStepDeploymentStep>
{
    private readonly IEnumerable<IRecipeDeploymentStep> _recipeSteps;
    private readonly IServiceProvider _serviceProvider;

    public RecipeStepDeploymentSource(
        IEnumerable<IRecipeDeploymentStep> recipeSteps,
        IServiceProvider serviceProvider)
    {
        _recipeSteps = recipeSteps;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessAsync(RecipeStepDeploymentStep step, DeploymentPlanResult result)
    {
        var recipeStep = _recipeSteps.FirstOrDefault(
            s => string.Equals(s.Name, step.RecipeStepName, StringComparison.OrdinalIgnoreCase));

        if (recipeStep is null)
        {
            return;
        }

        var context = new RecipeExportContext
        {
            ServiceProvider = _serviceProvider,
            Steps = result.Steps,
        };

        await recipeStep.ExportAsync(context);
    }
}
