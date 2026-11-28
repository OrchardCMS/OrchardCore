using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "CustomSettings";

    public static LocalizationSource Category => CommonLocalizationSources.Settings;

    public static LocalizationSource Title => CustomSettingsLocalizationSources.CustomSettings;

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
