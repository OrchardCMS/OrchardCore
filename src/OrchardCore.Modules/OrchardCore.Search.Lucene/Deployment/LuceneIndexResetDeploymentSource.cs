using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexResetDeploymentSource
    : DeploymentSourceBase<LuceneIndexResetDeploymentStep>
{
    protected override Task ProcessAsync(LuceneIndexResetDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll
            ? []
            : step.IndexNames;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
