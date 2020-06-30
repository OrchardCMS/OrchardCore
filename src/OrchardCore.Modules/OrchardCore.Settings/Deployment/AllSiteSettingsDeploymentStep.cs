using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    /// <summary>
    /// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllSiteSettingsDeploymentStep : DeploymentStep
    {
        public AllSiteSettingsDeploymentStep()
        {
            Name = "AllSiteSettings";
        }

        public string[] Settings { get; set; }
    }
}
