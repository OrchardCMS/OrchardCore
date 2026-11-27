using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class JsonRecipeDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "JsonRecipe";

    public static LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    public static LocalizationSource Title => DeploymentLocalizationSources.JsonRecipe;

    public string Json { get; set; }
}
