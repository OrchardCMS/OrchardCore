using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexRebuildDeploymentSource
    : DeploymentSourceBase<ElasticIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(ElasticIndexRebuildDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll ? [] : step.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "elastic-index-rebuild",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
