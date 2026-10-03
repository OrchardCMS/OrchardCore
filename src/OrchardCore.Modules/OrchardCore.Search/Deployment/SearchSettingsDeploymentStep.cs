using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Search.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SearchSettingsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<SearchSettingsDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<SearchSettingsDeploymentStep>("Search Settings");

    public SearchSettingsDeploymentStep()
    {
        Name = "SearchSettings";
        Category = s_category;
        Title = s_title;
    }
}
