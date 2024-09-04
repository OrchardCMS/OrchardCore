using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Deployment;

public class SearchSettingsDeploymentSource : IDeploymentSource
{
    private readonly ISiteService _siteService;

    public SearchSettingsDeploymentSource(ISiteService site)
    {
        _siteService = site;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not SearchSettingsDeploymentStep)
        {
            return;
        }

        var searchSettings = await _siteService.GetSettingsAsync<SearchSettings>();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["SearchSettings"] = JObject.FromObject(searchSettings),
        });
    }
}
