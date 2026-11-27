using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Placements.Deployment;

/// <summary>
/// Adds placements to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class PlacementsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "Placements";

    public static LocalizationSource Category => CommonLocalizationSources.Development;
}
