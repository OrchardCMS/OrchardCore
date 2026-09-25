using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsDeploymentStep : DeploymentStep
{
    public SiteSettingsDeploymentStep()
    {
        Name = nameof(SiteSettings);
    }

    public SiteSettingsDeploymentStep(IStringLocalizer<SiteSettingsDeploymentStep> S)
        : this()
    {
        Category = S["Configuration"];
        Title = S["Site Settings"];
    }

    public string[] Settings { get; set; }
}
