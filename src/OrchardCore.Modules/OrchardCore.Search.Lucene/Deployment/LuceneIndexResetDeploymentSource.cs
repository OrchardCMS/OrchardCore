using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexResetDeploymentSource : IDeploymentSource
{
    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var luceneIndexResetStep = step as LuceneIndexResetDeploymentStep;

        if (luceneIndexResetStep == null)
        {
            return Task.CompletedTask;
        }

        var indicesToReset = luceneIndexResetStep.IncludeAll ? [] : luceneIndexResetStep.IndexNames;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = luceneIndexResetStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
