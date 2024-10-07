using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexResetDeploymentSource
    : DeploymentSourceBase<LuceneIndexResetDeploymentStep>
{
    protected override Task ProcessAsync(DeploymentPlanResult result)
    {
        var indicesToReset = DeploymentStep.IncludeAll
            ? []
            : DeploymentStep.IndexNames;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
