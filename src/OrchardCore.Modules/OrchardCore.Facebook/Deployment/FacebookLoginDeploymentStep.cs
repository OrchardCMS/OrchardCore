using OrchardCore.Deployment;

namespace OrchardCore.Facebook.Deployment
{
    /// <summary>
    /// Adds Facebook Login settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class FacebookLoginDeploymentStep : DeploymentStep
    {
        public FacebookLoginDeploymentStep()
        {
            Name = "Facebook Login";
        }
    }
}
