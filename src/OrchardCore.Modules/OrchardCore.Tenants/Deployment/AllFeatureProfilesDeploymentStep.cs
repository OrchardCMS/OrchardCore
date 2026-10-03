using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllFeatureProfilesDeploymentStep>("Infrastructure");

    public AllFeatureProfilesDeploymentStep()
    {
        Name = "AllFeatureProfiles";
        Category = s_category;
    }
}
