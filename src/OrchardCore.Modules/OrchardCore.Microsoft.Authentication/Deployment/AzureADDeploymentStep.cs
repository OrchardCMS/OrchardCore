using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep
{
    public AzureADDeploymentStep()
    {
        Name = "Microsoft Entra ID";
        Category = MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
    }
}
