using OrchardCore.Deployment;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep
{
    public CustomUserSettingsDeploymentStep()
    {
        Name = "CustomUserSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
