using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<MicrosoftAccountDeploymentStep>("Microsoft Authentication");

    public MicrosoftAccountDeploymentStep()
    {
        Name = "MicrosoftAccount";
        Category = s_category;
    }
}
