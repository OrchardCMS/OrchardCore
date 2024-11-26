using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticsearchIndexRebuildDeploymentSource
    : DeploymentSourceBase<ElasticsearchIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(ElasticsearchIndexRebuildDeploymentStep step, DeploymentPlanResult result)
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
