using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Infrastructure", typeof(AllFeatureProfilesDeploymentStep));

    public AllFeatureProfilesDeploymentStep()
    {
        Name = "AllFeatureProfiles";
        Category = s_category;
    }
}
