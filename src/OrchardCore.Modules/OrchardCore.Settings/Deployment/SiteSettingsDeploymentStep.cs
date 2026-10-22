using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<SiteSettingsDeploymentStep>("Configuration");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<SiteSettingsDeploymentStep>("Site Settings");

    public SiteSettingsDeploymentStep()
    {
        Name = nameof(SiteSettings);
        Category = s_category;
        Title = s_title;
    }

    public string[] Settings { get; set; }
}
