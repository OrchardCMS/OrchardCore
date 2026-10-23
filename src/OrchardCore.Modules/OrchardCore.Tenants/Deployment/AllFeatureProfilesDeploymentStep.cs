using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tenants.Deployment;

public class AllFeatureProfilesDeploymentStep : DeploymentStep
{
    public override string Name => "AllFeatureProfiles";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Infrastructure;
}
