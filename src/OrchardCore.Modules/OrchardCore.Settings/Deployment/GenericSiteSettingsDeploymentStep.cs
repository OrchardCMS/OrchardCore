using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    /// <summary>
    /// Adds a generic site settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class GenericSiteSettingsDeploymentStep : DeploymentStep
    {
        public GenericSiteSettingsDeploymentStep()
        {
        }

        public GenericSiteSettingsDeploymentStep(string name, string title, string description)
        {
            Name = name;
            Title = title;
            Description = description;
        }

        public string Title { get; set; } // LocalizedString
        public string Description { get; set; }
    }
}
