using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "MicrosoftAccount";

    public static LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
}
