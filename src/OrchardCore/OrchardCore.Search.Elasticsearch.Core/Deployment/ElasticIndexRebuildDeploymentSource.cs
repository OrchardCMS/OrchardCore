using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
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

            var indicesToRebuild = elasticIndexRebuildStep.IncludeAll ? Array.Empty<string>() : elasticIndexRebuildStep.Indices;

            result.Steps.Add(new JObject(
                new JProperty("name", "elastic-index-rebuild"),
                new JProperty("includeAll", elasticIndexRebuildStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToRebuild))
            ));

            return Task.CompletedTask;
        }
    }
}
