using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticsearchIndexDeploymentSource
    : DeploymentSourceBase<ElasticsearchIndexDeploymentStep>
{
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

    public ElasticsearchIndexDeploymentSource(ElasticsearchIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    protected override async Task ProcessAsync(ElasticsearchIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = step.IncludeAll
            ? indexSettings.Select(x => x.IndexName).ToArray()
            : step.IndexNames;

        foreach (var index in indexSettings)
        {
            if (indicesToAdd.Contains(index.IndexName))
            {
                var indexSettingsDict = new Dictionary<string, ElasticIndexSettings>
                {
                    { index.IndexName, index },
                };

                data.Add(JObject.FromObject(indexSettingsDict));
            }
        }

        // Adding Elasticsearch settings.
        result.Steps.Add(new JsonObject
        {
            ["name"] = "ElasticIndexSettings",
            ["Indices"] = data,
        });
    }
}
