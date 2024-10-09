using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticSettingsDeploymentSource
    : DeploymentSourceBase<ElasticSettingsDeploymentStep>
{
    private readonly ElasticIndexingService _elasticIndexingService;

    public ElasticSettingsDeploymentSource(ElasticIndexingService elasticIndexingService)
    {
        _elasticIndexingService = elasticIndexingService;
    }

    protected override async Task ProcessAsync(ElasticSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var elasticSettings = await _elasticIndexingService.GetElasticSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["ElasticSettings"] = JObject.FromObject(elasticSettings),
        });
    }
}
