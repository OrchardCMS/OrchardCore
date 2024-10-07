using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public class LuceneIndexRebuildDeploymentSource
    : DeploymentSourceBase<LuceneIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(DeploymentPlanResult result)
    {
        var indicesToRebuild = DeploymentStep.IncludeAll
            ? []
            : DeploymentStep.IndexNames;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-rebuild",
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
