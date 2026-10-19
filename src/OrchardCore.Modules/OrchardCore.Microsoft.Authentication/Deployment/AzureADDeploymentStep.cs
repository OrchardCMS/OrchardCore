using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep
{
    public override string Name => "Microsoft Entra ID";

    [JsonIgnore]
    public override LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
}
