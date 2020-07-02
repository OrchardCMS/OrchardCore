using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class GenericSiteSettingsDeploymentStepFactory: IDeploymentStepFactory
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public GenericSiteSettingsDeploymentStepFactory(string name, string title, string description)
        {
            Name = name;
            Title = title;
            Description = description;
        }

        public DeploymentStep Create()
        {
            return new GenericSiteSettingsDeploymentStep(Name, Title, Description);
        }
    }
}
