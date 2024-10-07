using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneSettingsDeploymentSource
    : DeploymentSourceBase<LuceneSettingsDeploymentStep>
{
    private readonly LuceneIndexingService _luceneIndexingService;

    public LuceneSettingsDeploymentSource(LuceneIndexingService luceneIndexingService)
    {
        _luceneIndexingService = luceneIndexingService;
    }

    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

        // Adding Lucene settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["LuceneSettings"] = JObject.FromObject(luceneSettings),
        });
    }
}
