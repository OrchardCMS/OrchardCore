using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentSource(ISiteService siteService) : IDeploymentSource
{
    private readonly ISiteService _siteService = siteService;

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureAISearchSettingsDeploymentStep)
        {
            return;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var settings = site.As<AzureAISearchSettings>();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            [nameof(AzureAISearchSettings)] = JObject.FromObject(settings),
        });
    }
}
