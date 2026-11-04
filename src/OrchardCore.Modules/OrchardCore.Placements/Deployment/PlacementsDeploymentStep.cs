using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Placements.Deployment;

/// <summary>
/// Adds placements to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class PlacementsDeploymentStep : DeploymentStep
{
    public override string Name => "Placements";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Development;
}
