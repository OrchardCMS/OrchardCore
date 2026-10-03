using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdServerDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("OpenID Connect", typeof(OpenIdServerDeploymentStep));

    public OpenIdServerDeploymentStep()
    {
        Name = "OpenID Server";
        Category = s_category;
    }
}
