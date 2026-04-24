using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment;

public sealed class LuceneIndexRebuildDeploymentSource
    : DeploymentSourceBase<LuceneIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(LuceneIndexRebuildDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll
            ? []
            : step.IndexNames;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-rebuild",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
