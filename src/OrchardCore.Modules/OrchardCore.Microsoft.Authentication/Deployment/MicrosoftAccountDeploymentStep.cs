using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    public MicrosoftAccountDeploymentStep()
    {
        Name = "MicrosoftAccount";
    }

    public MicrosoftAccountDeploymentStep(IStringLocalizer<MicrosoftAccountDeploymentStep> S)
        : this()
    {
        Category = S["Microsoft Authentication"];
    }
}
