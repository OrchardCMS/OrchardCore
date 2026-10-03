using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Microsoft Authentication", typeof(AzureADDeploymentStep));

    public AzureADDeploymentStep()
    {
        Name = "Microsoft Entra ID";
        Category = s_category;
    }
}
