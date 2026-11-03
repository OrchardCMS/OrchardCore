using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep
{
    public override string Name => "JsonRecipe";

    [JsonIgnore]
    public override LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.JsonRecipe;

    public string Json { get; set; }
}
