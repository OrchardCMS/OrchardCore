using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexDeploymentSource : IDeploymentSource
{
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public ElasticIndexDeploymentSource(ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not ElasticIndexDeploymentStep elasticIndexStep)
        {
            return;
        }

        var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = elasticIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : elasticIndexStep.IndexNames;

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
