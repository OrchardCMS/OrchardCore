using OrchardCore.Deployment;

namespace OrchardCore.ThemeSettings.Deployment
{
    /// <summary>
    /// Adds theme settings to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class ThemeSettingsDeploymentStep : DeploymentStep
    {
        public ThemeSettingsDeploymentStep()
        {
            Name = "ThemeSettings";
        }
    }
}
