using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentSource(ISiteService siteService) : IDeploymentSource
{
    private readonly ISiteService _siteService = siteService;

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var settingsStep = step as AzureAISearchSettingsDeploymentStep;

        if (settingsStep == null)
        {
            return;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var settings = site.As<AzureAISearchSettings>();

        result.Steps.Add(new JObject(
            new JProperty("name", "Settings"),
            new JProperty(nameof(AzureAISearchSettings), JObject.FromObject(settings))
        ));
    }
}
