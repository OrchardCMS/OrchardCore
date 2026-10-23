using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Search.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SearchSettingsDeploymentStep : DeploymentStep
{
    public override string Name => "SearchSettings";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => SearchLocalizationSources.SearchSettings;
}
