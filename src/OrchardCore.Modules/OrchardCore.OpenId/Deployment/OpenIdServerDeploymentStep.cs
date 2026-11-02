using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdServerDeploymentStep : DeploymentStep
{
    public override string Name => "OpenID Server";

    [JsonIgnore]
    public override LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;
}
