using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexResetDeploymentSource
    : DeploymentSourceBase<ElasticIndexResetDeploymentStep>
{
    protected override Task ProcessAsync(ElasticIndexResetDeploymentStep step, DeploymentPlanResult result)
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
