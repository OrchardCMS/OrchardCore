using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexResetDeploymentSource : IDeploymentSource
{
    public ElasticIndexResetDeploymentSource()
    {
    }

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticIndexResetStep = step as ElasticIndexResetDeploymentStep;

        if (elasticIndexResetStep == null)
        {
            return Task.CompletedTask;
        }

        var indicesToReset = elasticIndexResetStep.IncludeAll ? [] : elasticIndexResetStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "lucene-index-reset",
            ["includeAll"] = elasticIndexResetStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
