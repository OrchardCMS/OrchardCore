using OrchardCore.Deployment;

namespace OrchardCore.CustomSettings.Deployment
{
    public class CustomSettingsDeploymentStep : DeploymentStep
    {
        public CustomSettingsDeploymentStep()
        {
            Name = "CustomSettings";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] SettingsTypeNames { get; set; }
    }
}
