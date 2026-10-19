using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Search.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SearchSettingsDeploymentStep : DeploymentStep
{
    public SearchSettingsDeploymentStep()
    {
        Name = "SearchSettings";
        Category = CommonLocalizationSources.Search;
        Title = SearchLocalizationSources.SearchSettings;
    }
}
