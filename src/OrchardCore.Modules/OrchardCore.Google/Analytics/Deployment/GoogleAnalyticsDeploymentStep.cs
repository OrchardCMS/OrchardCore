using OrchardCore.Deployment;

namespace OrchardCore.Google.Analytics.Deployment
{
    /// <summary>
    /// Adds Google Analytics settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class GoogleAnalyticsDeploymentStep : DeploymentStep
    {
        public GoogleAnalyticsDeploymentStep()
        {
            Name = "Google Analytics";
        }
    }
}
