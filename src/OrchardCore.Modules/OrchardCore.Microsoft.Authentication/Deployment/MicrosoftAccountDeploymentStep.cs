using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    public MicrosoftAccountDeploymentStep()
    {
        Name = "MicrosoftAccount";
        Category = LocalizedString.Create("Microsoft Authentication");
    }
}
