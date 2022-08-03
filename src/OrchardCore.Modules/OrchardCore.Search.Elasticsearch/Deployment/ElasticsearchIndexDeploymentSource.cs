using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Model;

namespace OrchardCore.Search.Elasticsearch.Deployment
{
    public class ElasticsearchIndexDeploymentSource : IDeploymentSource
    {
        private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

        public ElasticsearchIndexDeploymentSource(ElasticsearchIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var elasticIndexStep = step as ElasticsearchIndexDeploymentStep;

            if (elasticIndexStep == null)
            {
                return;
            }

            var indexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

            var data = new JArray();
            var indicesToAdd = elasticIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : elasticIndexStep.IndexNames;

            foreach (var index in indexSettings)
            {
                if (indicesToAdd.Contains(index.IndexName))
                {
                    var indexSettingsDict = new Dictionary<string, ElasticsearchIndexSettings>();
                    indexSettingsDict.Add(index.IndexName, index);
                    data.Add(JObject.FromObject(indexSettingsDict));
                }
            }

            // Adding Elasticsearch settings
            result.Steps.Add(new JObject(
                new JProperty("name", "elasticsearch-index"),
                new JProperty("Indices", data)
            ));
        }
    }
}
