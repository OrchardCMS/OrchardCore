namespace OrchardCore.Deployment;

/// <summary>
/// Factory that creates <see cref="RecipeStepDeploymentStep"/> instances for a specific recipe step.
/// Each factory represents one recipe step type in the deployment UI.
/// </summary>
public sealed class RecipeStepDeploymentStepFactory : IDeploymentStepFactory
{
    private readonly string _recipeStepName;

    public RecipeStepDeploymentStepFactory(string recipeStepName)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipeStepName);
        _recipeStepName = recipeStepName;
    }

    /// <summary>
    /// Gets the unique name for this factory, prefixed to distinguish from other deployment step types.
    /// </summary>
    public string Name => $"{nameof(RecipeStepDeploymentStep)}_{_recipeStepName}";

    public DeploymentStep Create()
    {
        return new RecipeStepDeploymentStep
        {
            Name = Name,
            RecipeStepName = _recipeStepName,
        };
    }
}
