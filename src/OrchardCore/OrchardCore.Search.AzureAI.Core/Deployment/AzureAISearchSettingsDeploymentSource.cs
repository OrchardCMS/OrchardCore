using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentSource : IDeploymentSource
{
    private readonly ISiteService _siteService;

    public AzureAISearchSettingsDeploymentSource(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticSettingsStep = step as AzureAISearchSettingsDeploymentStep;

        if (elasticSettingsStep == null)
        {
            return;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var elasticSettings = site.As<AzureAISearchSettings>();

        result.Steps.Add(new JObject(
            new JProperty("name", "Settings"),
            new JProperty(nameof(AzureAISearchSettings), JObject.FromObject(elasticSettings))
        ));
    }
}
