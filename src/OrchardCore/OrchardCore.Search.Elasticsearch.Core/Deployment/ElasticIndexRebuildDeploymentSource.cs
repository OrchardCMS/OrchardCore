using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexRebuildDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexRebuildStep = step as ElasticIndexRebuildDeploymentStep;
            if (elasticIndexRebuildStep == null)
            {
                return Task.CompletedTask;
            }

            var indicesToRebuild = elasticIndexRebuildStep.IncludeAll ? Array.Empty<string>() : elasticIndexRebuildStep.Indices;

            result.AddStep("elastic-index-rebuild", new []
            {
                new KeyValuePair<string, JsonNode>("includeAll", elasticIndexRebuildStep.IncludeAll),
                new KeyValuePair<string, JsonNode>("Indices", JsonSerializer.SerializeToNode(indicesToRebuild)),
            });

            return Task.CompletedTask;
        }
    }
}
