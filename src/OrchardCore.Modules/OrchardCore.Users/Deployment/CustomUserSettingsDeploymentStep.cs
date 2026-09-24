using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep
{
    public CustomUserSettingsDeploymentStep()
    {
        Name = "CustomUserSettings";
        Category = LocalizedString.Create("Security");
        Title = LocalizedString.Create("Custom User Settings");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
