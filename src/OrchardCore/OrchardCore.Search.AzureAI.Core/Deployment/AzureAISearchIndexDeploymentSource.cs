using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentSource(AzureAISearchIndexSettingsService indexSettingsService) : IDeploymentSource
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService = indexSettingsService;

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureAISearchIndexDeploymentStep indexStep)
        {
            return;
        }

        var indexSettings = await _indexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = indexStep.IncludeAll
            ? indexSettings.Select(x => x.IndexName).ToArray()
            : indexStep.IndexNames;

        foreach (var index in indexSettings)
        {
            if (index.IndexName == null || !indicesToAdd.Contains(index.IndexName))
            {
                continue;
            }

            var indexInfo = GetIndexInfo(index);

            data.Add(JObject.FromObject(indexInfo));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = step.Name,
            ["Indices"] = data,
        });
    }

    private static AzureAISearchIndexInfo GetIndexInfo(AzureAISearchIndexSettings settings)
        => new()
        {
            IndexName = settings.IndexName,
            AnalyzerName = settings.AnalyzerName,
            QueryAnalyzerName = settings.QueryAnalyzerName,
            IndexedContentTypes = settings.IndexedContentTypes ?? [],
            IndexLatest = settings.IndexLatest,
            Culture = settings.Culture,
        };
}

public class AzureAISearchIndexInfo
{
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }
}
