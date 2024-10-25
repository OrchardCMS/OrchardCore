using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Deployment;

public class SearchSettingsDeploymentSource
    : DeploymentSourceBase<SearchSettingsDeploymentStep>
{
    private readonly ISiteService _siteService;

    public SearchSettingsDeploymentSource(ISiteService site)
    {
        _siteService = site;
    }

    protected override async Task ProcessAsync(SearchSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var searchSettings = await _siteService.GetSettingsAsync<SearchSettings>();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["SearchSettings"] = JObject.FromObject(searchSettings),
        });
    }
}
