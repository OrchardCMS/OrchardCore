using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.JsonRecipe;

    public JsonRecipeDeploymentStep()
    {
        Name = "JsonRecipe";
    }

    public string Json { get; set; }
}
