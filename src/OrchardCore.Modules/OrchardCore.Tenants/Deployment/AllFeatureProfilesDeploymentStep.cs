using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep
{
    public AllFeatureProfilesDeploymentStep()
    {
        Name = "AllFeatureProfiles";
        Category = CommonLocalizationSources.Infrastructure;
    }
}
