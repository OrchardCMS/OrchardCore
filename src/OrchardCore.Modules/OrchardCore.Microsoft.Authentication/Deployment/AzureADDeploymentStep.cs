using OrchardCore.Deployment;

namespace OrchardCore.Microsoft.Authentication.Deployment
{
    /// <summary>
    /// Adds Google Analytics settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AzureADDeploymentStep : DeploymentStep
    {
        public AzureADDeploymentStep()
        {
            Name = "Azure AD";
        }
    }
}
