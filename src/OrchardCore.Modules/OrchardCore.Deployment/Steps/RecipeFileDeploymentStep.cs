using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a Recipe file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class RecipeFileDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Deployment", typeof(RecipeFileDeploymentStep));
    private static readonly LocalizationSource s_title = new("Recipe File", typeof(RecipeFileDeploymentStep));

    public RecipeFileDeploymentStep()
    {
        Name = nameof(RecipeFileDeploymentStep);
        Category = s_category;
        Title = s_title;
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
