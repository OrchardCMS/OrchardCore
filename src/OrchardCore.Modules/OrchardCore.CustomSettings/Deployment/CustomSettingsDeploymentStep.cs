using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Settings;

    [JsonIgnore]
    public override LocalizationSource Title => CustomSettingsLocalizationSources.CustomSettings;

    public CustomSettingsDeploymentStep()
    {
        Name = "CustomSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
