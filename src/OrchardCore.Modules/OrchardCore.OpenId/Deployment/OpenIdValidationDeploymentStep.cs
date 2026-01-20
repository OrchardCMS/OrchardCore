using OrchardCore.Deployment;

namespace OrchardCore.OpenId.Deployment
{
    /// <summary>
    /// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class OpenIdValidationDeploymentStep : DeploymentStep
    {
        public OpenIdValidationDeploymentStep()
        {
            Name = "OpenID Validation";
        }
    }
}
