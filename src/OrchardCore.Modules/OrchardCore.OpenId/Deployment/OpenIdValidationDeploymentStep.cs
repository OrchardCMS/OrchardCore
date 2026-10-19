using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdValidationDeploymentStep : DeploymentStep
{
    public override string Name => "OpenID Validation";

    [JsonIgnore]
    public override LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;
}
