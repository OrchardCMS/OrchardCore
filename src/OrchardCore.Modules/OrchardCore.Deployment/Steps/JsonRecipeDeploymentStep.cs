using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep
{
    public JsonRecipeDeploymentStep()
    {
        Name = "JsonRecipe";
        Category = LocalizedString.Create("Deployment");
    }

    public string Json { get; set; }
}
