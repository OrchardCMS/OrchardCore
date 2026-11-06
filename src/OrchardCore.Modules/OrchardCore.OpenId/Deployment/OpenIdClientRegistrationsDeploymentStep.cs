using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds the additional OpenID Connect client registrations to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class OpenIdClientRegistrationsDeploymentStep : DeploymentStep
{
    public override string Name => "OpenID Client Registrations";

    [JsonIgnore]
    public override LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;
}
