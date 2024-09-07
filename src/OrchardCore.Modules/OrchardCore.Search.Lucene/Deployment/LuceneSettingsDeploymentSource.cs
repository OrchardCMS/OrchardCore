using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneSettingsDeploymentSource : IDeploymentSource
{
    private readonly LuceneIndexingService _luceneIndexingService;

    public LuceneSettingsDeploymentSource(LuceneIndexingService luceneIndexingService)
    {
        _luceneIndexingService = luceneIndexingService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not LuceneSettingsDeploymentStep)
        {
            return;
        }

        var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

        // Adding Lucene settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["LuceneSettings"] = JObject.FromObject(luceneSettings),
        });
    }
}
