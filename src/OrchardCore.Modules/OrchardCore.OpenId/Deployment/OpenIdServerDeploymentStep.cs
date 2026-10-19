using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdServerDeploymentStep : DeploymentStep
{
    public OpenIdServerDeploymentStep()
    {
        Name = "OpenID Server";
        Category = OpenIdLocalizationSources.OpenIdConnect;
    }
}
