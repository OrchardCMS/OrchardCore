using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Search.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SearchSettingsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "SearchSettings";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => SearchLocalizationSources.SearchSettings;
}
