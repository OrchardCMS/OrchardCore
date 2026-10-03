using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds media profiles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllMediaProfilesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(AllMediaProfilesDeploymentStep));

    public AllMediaProfilesDeploymentStep()
    {
        Name = "AllMediaProfiles";
        Category = s_category;
    }
}
