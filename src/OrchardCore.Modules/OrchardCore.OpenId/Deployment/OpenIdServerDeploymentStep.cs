using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdServerDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;

    public OpenIdServerDeploymentStep()
    {
        Name = "OpenID Server";
    }
}
