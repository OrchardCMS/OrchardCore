using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep
{
    public AllFeatureProfilesDeploymentStep()
    {
        Name = "AllFeatureProfiles";
        Category = LocalizedString.Create("Infrastructure");
    }
}
