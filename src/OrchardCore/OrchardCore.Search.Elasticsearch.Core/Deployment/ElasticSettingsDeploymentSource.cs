using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ElasticIndexingService _elasticIndexingService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ElasticSettingsDeploymentSource(
            ElasticIndexingService elasticIndexingService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _elasticIndexingService = elasticIndexingService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticSettingsStep = step as ElasticSettingsDeploymentStep;

            if (elasticSettingsStep == null)
            {
                return;
            }

            var elasticSettings = await _elasticIndexingService.GetElasticSettingsAsync();

            // Adding Elasticsearch settings
            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                ["ElasticSettings"] = JObject.FromObject(elasticSettings, _jsonSerializerOptions),
            });
        }
    }
}
