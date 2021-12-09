using OrchardCore.Deployment;

namespace OrchardCore.OpenId.Deployment
{
    /// <summary>
    /// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class OpenIdServerDeploymentStep : DeploymentStep
    {
        public OpenIdServerDeploymentStep()
        {
            Name = "OpenID Server";
        }
    }
}
