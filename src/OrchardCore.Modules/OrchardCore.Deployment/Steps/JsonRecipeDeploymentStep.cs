using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Deployment", typeof(JsonRecipeDeploymentStep));
    private static readonly LocalizationSource s_title = new("JSON Recipe", typeof(JsonRecipeDeploymentStep));

    public JsonRecipeDeploymentStep()
    {
        Name = "JsonRecipe";
        Category = s_category;
        Title = s_title;
    }

    public string Json { get; set; }
}
