using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexDeploymentSource
    : DeploymentSourceBase<LuceneIndexDeploymentStep>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexDeploymentSource(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    protected override async Task ProcessAsync(LuceneIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

        var data = new JsonArray();
        var indicesToAdd = step.IncludeAll
            ? indexSettings.Select(x => x.IndexName).ToArray()
            : step.IndexNames;

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
