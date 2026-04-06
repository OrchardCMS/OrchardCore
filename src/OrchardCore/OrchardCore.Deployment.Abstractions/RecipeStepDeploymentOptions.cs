namespace OrchardCore.Deployment;

/// <summary>
/// Stores display metadata for recipe-based deployment steps.
/// </summary>
public sealed class RecipeStepDeploymentOptions
{
    /// <summary>
    /// Gets the registered recipe step deployment metadata, keyed by recipe step name.
    /// </summary>
    public Dictionary<string, RecipeStepDeploymentInfo> Steps { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Display metadata for a recipe-based deployment step.
/// </summary>
public sealed class RecipeStepDeploymentInfo
{
    /// <summary>
    /// Gets or sets the display title for this deployment step.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description shown in the deployment step thumbnail and summary.
    /// </summary>
    public string Description { get; set; }
}
