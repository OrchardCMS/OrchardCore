using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexResetDeploymentSource : IDeploymentSource
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ElasticIndexResetDeploymentSource(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexResetStep = step as ElasticIndexResetDeploymentStep;

            if (elasticIndexResetStep == null)
            {
                return;
            }

            var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

            var data = new JArray();
            var indicesToReset = elasticIndexResetStep.IncludeAll ? Array.Empty<string>() : elasticIndexResetStep.Indices;

            result.Steps.Add(new JObject(
            new JProperty("name", "lucene-index-reset"),
                new JProperty("includeAll", elasticIndexResetStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToReset))
            ));
        }
    }
}
