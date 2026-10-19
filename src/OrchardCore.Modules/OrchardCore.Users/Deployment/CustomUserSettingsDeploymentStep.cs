using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentStep : DeploymentStep
{
    public override string Name => "CustomUserSettings";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Security;

    [JsonIgnore]
    public override LocalizationSource Title => UsersLocalizationSources.CustomUserSettings;

    public bool IncludeAll { get; set; } = true;

    public string[] SettingsTypeNames { get; set; }
}
