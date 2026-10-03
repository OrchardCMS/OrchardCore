using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Security", typeof(CustomUserSettingsDeploymentStep));
    private static readonly LocalizationSource s_title = new("Custom User Settings", typeof(CustomUserSettingsDeploymentStep));

    public CustomUserSettingsDeploymentStep()
    {
        Name = "CustomUserSettings";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
