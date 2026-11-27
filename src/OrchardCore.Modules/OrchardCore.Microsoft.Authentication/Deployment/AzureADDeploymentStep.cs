using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Microsoft.Authentication.Deployment;

/// <summary>
/// Adds Microsoft Entra ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AzureADDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "Microsoft Entra ID";

    public static LocalizationSource Category => MicrosoftAuthenticationLocalizationSources.MicrosoftAuthentication;
}
