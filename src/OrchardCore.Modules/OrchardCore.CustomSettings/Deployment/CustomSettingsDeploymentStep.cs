using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentStep : DeploymentStep
{
    public CustomSettingsDeploymentStep()
    {
        Name = "CustomSettings";
    }

    public CustomSettingsDeploymentStep(IStringLocalizer<CustomSettingsDeploymentStep> S)
        : this()
    {
        Category = S["Settings"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
