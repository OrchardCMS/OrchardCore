using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexRebuildDeploymentSource : IDeploymentSource
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ElasticIndexRebuildDeploymentSource(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexRebuildStep = step as ElasticIndexRebuildDeploymentStep;

            if (elasticIndexRebuildStep == null)
            {
                return;
            }

            var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

            var data = new JArray();
            var indicesToRebuild = elasticIndexRebuildStep.IncludeAll ? Array.Empty<string>() : elasticIndexRebuildStep.Indices;

            result.Steps.Add(new JObject(
                new JProperty("name", "elastic-index-rebuild"),
                new JProperty("includeAll", elasticIndexRebuildStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToRebuild))
            ));
        }
    }
}
