using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Placements.Deployment;

/// <summary>
/// Adds placements to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class PlacementsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<PlacementsDeploymentStep>("Development");

    public PlacementsDeploymentStep()
    {
        Name = "Placements";
        Category = s_category;
    }
}
