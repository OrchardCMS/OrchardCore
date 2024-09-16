using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexDeploymentSource : IDeploymentSource
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexDeploymentSource(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not LuceneIndexDeploymentStep luceneIndexStep)
        {
            return;
        }

        var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = luceneIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : luceneIndexStep.IndexNames;

        foreach (var index in indexSettings)
        {
            if (indicesToAdd.Contains(index.IndexName))
            {
                var indexSettingsDict = new Dictionary<string, LuceneIndexSettings>
                {
                    { index.IndexName, index },
                };

                data.Add(JObject.FromObject(indexSettingsDict));
            }
        }

        // Adding Lucene settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index",
            ["Indices"] = data,
        });
    }
}
