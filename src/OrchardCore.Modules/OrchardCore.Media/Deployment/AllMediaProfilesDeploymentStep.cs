using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds media profiles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllMediaProfilesDeploymentStep : DeploymentStep
{
    public override string Name => "AllMediaProfiles";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;
}
