using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentSource : IDeploymentSource
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;

    public AzureAISearchIndexDeploymentSource(AzureAISearchIndexSettingsService indexSettingsService)
    {
        _indexSettingsService = indexSettingsService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticIndexStep = step as AzureAISearchIndexDeploymentStep;

        if (elasticIndexStep == null)
        {
            return;
        }

        var indexSettings = await _indexSettingsService.GetSettingsAsync();

        var data = new JArray();
        var indicesToAdd = elasticIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : elasticIndexStep.IndexNames;

        foreach (var index in indexSettings)
        {
            if (indicesToAdd.Contains(index.IndexName))
            {
                var indexSettingsDict = new Dictionary<string, AzureAISearchIndexSettings>
                {
                    { index.IndexName, index },
                };

                data.Add(JObject.FromObject(indexSettingsDict));
            }
        }

        result.Steps.Add(new JObject(
            new JProperty("name", nameof(AzureAISearchIndexSettings)),
            new JProperty("Indices", data)
        ));
    }
}
