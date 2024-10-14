using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentSource(ISiteService siteService)
    : DeploymentSourceBase<AzureAISearchSettingsDeploymentStep>
{
    private readonly ISiteService _siteService = siteService;

    protected override async Task ProcessAsync(AzureAISearchSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var settings = await _siteService.GetSettingsAsync<AzureAISearchSettings>();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["AzureAISearchSettings"] = JObject.FromObject(settings),
        });
    }
}
