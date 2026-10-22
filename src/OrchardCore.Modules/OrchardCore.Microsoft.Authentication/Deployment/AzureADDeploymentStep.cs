using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AzureADDeploymentStep>("Microsoft Authentication");

    public AzureADDeploymentStep()
    {
        Name = "Microsoft Entra ID";
        Category = s_category;
    }
}
