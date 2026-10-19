using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;

    public AzureADDeploymentStep()
    {
        Name = "Microsoft Entra ID";
    }
}
