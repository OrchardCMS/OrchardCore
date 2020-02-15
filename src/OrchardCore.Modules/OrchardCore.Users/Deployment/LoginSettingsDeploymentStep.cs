using OrchardCore.Deployment;

namespace OrchardCore.Users.Deployment
{
    /// <summary>
    /// Adds layers to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class LoginSettingsDeploymentStep : DeploymentStep
    {
        public LoginSettingsDeploymentStep()
        {
            Name = "LoginSettings";
        }
    }
}
