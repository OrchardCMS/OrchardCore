using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticSettingsDeploymentSource
    : DeploymentSourceBase<ElasticSettingsDeploymentStep>
{
    private readonly ElasticsearchIndexSettingsService _elasticsearchIndexSettingsService;

    public ElasticSettingsDeploymentSource(ElasticsearchIndexSettingsService elasticsearchIndexSettingsService)
    {
        _elasticsearchIndexSettingsService = elasticsearchIndexSettingsService;
    }

    protected override async Task ProcessAsync(ElasticSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var elasticSettings = await _elasticsearchIndexSettingsService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["ElasticSettings"] = JObject.FromObject(elasticSettings),
        });
    }
}
