using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "CustomUserSettings";

    public static LocalizationSource Category => CommonLocalizationSources.Security;

    public static LocalizationSource Title => UsersLocalizationSources.CustomUserSettings;

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
