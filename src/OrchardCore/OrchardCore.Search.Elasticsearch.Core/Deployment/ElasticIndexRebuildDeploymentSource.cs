using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexRebuildDeploymentSource
    : DeploymentSourceBase<ElasticIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = DeploymentStep.IncludeAll ? [] : DeploymentStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "elastic-index-rebuild",
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
