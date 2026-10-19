using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Facebook.Deployment;

/// <summary>
/// Adds Facebook Login settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class FacebookLoginDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => FacebookLocalizationSources.Meta;

    public FacebookLoginDeploymentStep()
    {
        Name = "Facebook Login";
    }
}
