using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => nameof(SiteSettings);

    public static LocalizationSource Category => CommonLocalizationSources.Configuration;

    public static LocalizationSource Title => SettingsLocalizationSources.SiteSettings;

    public string[] Settings { get; set; }
}
