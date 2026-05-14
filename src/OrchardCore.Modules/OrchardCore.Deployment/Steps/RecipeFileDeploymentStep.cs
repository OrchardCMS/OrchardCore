using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a Recipe file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class RecipeFileDeploymentStep : DeploymentStep
{
    public RecipeFileDeploymentStep()
    {
        Name = nameof(RecipeFileDeploymentStep);
    }

    public RecipeFileDeploymentStep(IStringLocalizer<RecipeFileDeploymentStep> S)
        : this()
    {
        Category = S["Deployment"];
    }

    public string RecipeName { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string Author { get; set; }

    public string WebSite { get; set; }

    public string Version { get; set; }

    public bool IsSetupRecipe { get; set; }

    public string Categories { get; set; }

    public string Tags { get; set; }
}
