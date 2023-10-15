using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexResetDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexResetStep = step as ElasticIndexResetDeploymentStep;

            if (elasticIndexResetStep == null)
            {
                return Task.CompletedTask;
            }

            var indicesToReset = elasticIndexResetStep.IncludeAll ? Array.Empty<string>() : elasticIndexResetStep.Indices;


            result.AddStep("lucene-index-reset", new []
            {
                new KeyValuePair<string, JsonNode>("includeAll", elasticIndexResetStep.IncludeAll),
                new KeyValuePair<string, JsonNode>("Indices", JsonSerializer.SerializeToNode(indicesToReset)),
            });

            return Task.CompletedTask;
        }
    }
}
