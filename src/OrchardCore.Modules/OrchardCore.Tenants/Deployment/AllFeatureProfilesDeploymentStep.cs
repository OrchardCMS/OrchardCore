using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllFeatureProfiles";

    public static LocalizationSource Category => CommonLocalizationSources.Infrastructure;
}
