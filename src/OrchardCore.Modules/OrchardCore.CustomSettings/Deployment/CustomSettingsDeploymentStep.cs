using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<CustomSettingsDeploymentStep>("Settings");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<CustomSettingsDeploymentStep>("Custom Settings");

    public CustomSettingsDeploymentStep()
    {
        Name = "CustomSettings";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
