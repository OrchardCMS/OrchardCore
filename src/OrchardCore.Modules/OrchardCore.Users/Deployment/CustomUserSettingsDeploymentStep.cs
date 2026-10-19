using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep
{
    public CustomUserSettingsDeploymentStep()
    {
        Name = "CustomUserSettings";
        Category = CommonLocalizationSources.Security;
        Title = UsersLocalizationSources.CustomUserSettings;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
