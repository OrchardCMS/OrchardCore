using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticSettingsDeploymentSource : IDeploymentSource
{
    private readonly ElasticIndexingService _elasticIndexingService;

    public ElasticSettingsDeploymentSource(ElasticIndexingService elasticIndexingService)
    {
        _elasticIndexingService = elasticIndexingService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not ElasticSettingsDeploymentStep)
        {
            return;
        }

        var elasticSettings = await _elasticIndexingService.GetElasticSettingsAsync();

        // Adding Elasticsearch settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["ElasticSettings"] = JObject.FromObject(elasticSettings),
        });
    }
}
