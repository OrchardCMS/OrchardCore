using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexResetDeploymentSource
    : DeploymentSourceBase<ElasticIndexResetDeploymentStep>
{
    public override Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        var indicesToReset = DeploymentStep.IncludeAll ? [] : DeploymentStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
