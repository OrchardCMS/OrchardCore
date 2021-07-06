using OrchardCore.Deployment;

namespace OrchardCore.Google.TagManager.Deployment
{
    /// <summary>
    /// Adds Google TagManager settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class GoogleTagManagerDeploymentStep : DeploymentStep
    {
        public GoogleTagManagerDeploymentStep()
        {
            Name = "Google Tag Manager";
        }
    }
}
