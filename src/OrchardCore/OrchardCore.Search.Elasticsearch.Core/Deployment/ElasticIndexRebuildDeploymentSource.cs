using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public class ElasticIndexRebuildDeploymentSource : IDeploymentSource
{
    public ElasticIndexRebuildDeploymentSource()
    {
    }

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticIndexRebuildStep = step as ElasticIndexRebuildDeploymentStep;
        if (elasticIndexRebuildStep == null)
        {
            return Task.CompletedTask;
        }

        var indicesToRebuild = elasticIndexRebuildStep.IncludeAll ? [] : elasticIndexRebuildStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "elastic-index-rebuild",
            ["includeAll"] = elasticIndexRebuildStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
