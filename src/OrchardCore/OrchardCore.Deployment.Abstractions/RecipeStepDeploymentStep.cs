namespace OrchardCore.Deployment;

/// <summary>
/// A deployment step that delegates to an <see cref="OrchardCore.Recipes.Services.IRecipeDeploymentStep"/>
/// for export. This enables automatic deployment step creation from recipe steps without requiring
/// custom Source/Step/Driver classes.
/// </summary>
public sealed class RecipeStepDeploymentStep : DeploymentStep
{
    /// <summary>
    /// Gets or sets the name of the recipe step to delegate to during export.
    /// </summary>
    public string RecipeStepName { get; set; }
}
