using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    public MicrosoftAccountDeploymentStep()
    {
        Name = "MicrosoftAccount";
        Category = MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
    }
}
