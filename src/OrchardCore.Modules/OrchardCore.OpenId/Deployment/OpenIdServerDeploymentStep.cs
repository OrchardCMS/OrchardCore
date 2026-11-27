using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdServerDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "OpenID Server";

    public static LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;
}
