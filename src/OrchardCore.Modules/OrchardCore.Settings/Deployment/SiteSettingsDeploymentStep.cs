using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds the current site settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsDeploymentStep : DeploymentStep
{
    public override string Name => nameof(SiteSettings);

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Configuration;

    [JsonIgnore]
    public override LocalizationSource Title => SettingsLocalizationSources.SiteSettings;

    public string[] Settings { get; set; }
}
