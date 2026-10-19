using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;

    public MicrosoftAccountDeploymentStep()
    {
        Name = "MicrosoftAccount";
    }
}
