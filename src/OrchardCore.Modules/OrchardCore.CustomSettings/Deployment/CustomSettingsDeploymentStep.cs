using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentStep : DeploymentStep
{
    public CustomSettingsDeploymentStep()
    {
        Name = "CustomSettings";
        Category = CommonLocalizationSources.Settings;
        Title = CustomSettingsLocalizationSources.CustomSettings;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
