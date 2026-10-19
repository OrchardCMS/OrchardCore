using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Placements.Deployment;

/// <summary>
/// Adds placements to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class PlacementsDeploymentStep : DeploymentStep
{
    public PlacementsDeploymentStep()
    {
        Name = "Placements";
        Category = CommonLocalizationSources.Development;
    }
}
