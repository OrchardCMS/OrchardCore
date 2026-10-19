using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsDeploymentStep : DeploymentStep
{
    public SiteSettingsDeploymentStep()
    {
        Name = nameof(SiteSettings);
        Category = CommonLocalizationSources.Configuration;
        Title = SettingsLocalizationSources.SiteSettings;
    }

    public string[] Settings { get; set; }
}
