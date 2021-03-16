using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    /// <summary>
    /// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class SiteSettingsDeploymentStep : DeploymentStep
    {
        public SiteSettingsDeploymentStep()
        {
            Name = nameof(SiteSettings);
        }

        public string[] Settings { get; set; }
    }
}
