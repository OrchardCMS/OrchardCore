using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexDeploymentSource : IDeploymentSource
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ElasticIndexDeploymentSource(
            ElasticIndexSettingsService elasticIndexSettingsService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexStep = step as ElasticIndexDeploymentStep;

            if (elasticIndexStep == null)
            {
                return;
            }

            var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

            var data = new JsonArray();
            var indicesToAdd = elasticIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : elasticIndexStep.IndexNames;

            foreach (var index in indexSettings)
            {
                if (indicesToAdd.Contains(index.IndexName))
                {
                    var indexSettingsDict = new Dictionary<string, ElasticIndexSettings>
                    {
                        { index.IndexName, index },
                    };

                    data.Add(JObject.FromObject(indexSettingsDict, _jsonSerializerOptions));
                }
            }

            // Adding Elasticsearch settings.
            result.Steps.Add(new JsonObject
            {
                ["name"] = "ElasticIndexSettings",
                ["Indices"] = data,
            });
        }
    }
}
