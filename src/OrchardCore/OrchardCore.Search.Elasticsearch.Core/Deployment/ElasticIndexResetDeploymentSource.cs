using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexResetDeploymentSource : IDeploymentSource
    {
        public ElasticIndexResetDeploymentSource()
        {
        }

        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not ElasticIndexResetDeploymentStep elasticIndexResetStep)
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
}
