using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentSource(AzureAISearchIndexSettingsService indexSettingsService, IOptions<JsonSerializerOptions> jsonSerializerOptions) : IDeploymentSource
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService = indexSettingsService;
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions.Value;

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureAISearchIndexDeploymentStep settingsStep)
        {
            return;
        }

        var indexSettings = await _indexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = settingsStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : settingsStep.IndexNames;

        foreach (var index in indexSettings)
        {
            if (indicesToAdd.Contains(index.IndexName))
            {
                var indexSettingsDict = new Dictionary<string, AzureAISearchIndexSettings>
                {
                    { index.IndexName, index },
                };

                data.Add(JObject.FromObject(indexSettingsDict, _jsonSerializerOptions));
            }
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = nameof(AzureAISearchIndexSettings),
            ["Indices"] = data,
        });
    }
}
