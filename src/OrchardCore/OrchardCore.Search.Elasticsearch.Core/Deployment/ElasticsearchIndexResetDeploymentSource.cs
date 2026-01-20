using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticsearchIndexResetDeploymentSource
    : DeploymentSourceBase<ElasticsearchIndexResetDeploymentStep>
{
    protected override Task ProcessAsync(ElasticsearchIndexResetDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll ? [] : step.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
