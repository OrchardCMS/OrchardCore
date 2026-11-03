using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    public override string Name => "MicrosoftAccount";

    [JsonIgnore]
    public override LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
}
