using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<JsonRecipeDeploymentStep>("Deployment");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<JsonRecipeDeploymentStep>("JSON Recipe");

    public JsonRecipeDeploymentStep()
    {
        Name = "JsonRecipe";
        Category = s_category;
        Title = s_title;
    }

    public string Json { get; set; }
}
