using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
            var elasticIndexResetStep = step as ElasticIndexResetDeploymentStep;

            if (elasticIndexResetStep == null)
            {
                return Task.CompletedTask;
            }

            var indicesToReset = elasticIndexResetStep.IncludeAll ? Array.Empty<string>() : elasticIndexResetStep.Indices;

            result.Steps.Add(new JObject(
            new JProperty("name", "lucene-index-reset"),
                new JProperty("includeAll", elasticIndexResetStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToReset))
            ));

            return Task.CompletedTask;
        }
    }
}
