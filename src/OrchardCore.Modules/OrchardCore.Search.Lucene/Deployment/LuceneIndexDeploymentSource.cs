using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public LuceneIndexDeploymentSource(
            LuceneIndexSettingsService luceneIndexSettingsService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexStep = step as LuceneIndexDeploymentStep;

            if (luceneIndexStep == null)
            {
                return;
            }

            var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

            var data = new JsonArray();
            var indicesToAdd = luceneIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : luceneIndexStep.IndexNames;

            foreach (var index in indexSettings)
            {
                if (indicesToAdd.Contains(index.IndexName))
                {
                    var indexSettingsDict = new Dictionary<string, LuceneIndexSettings>
                    {
                        { index.IndexName, index },
                    };

                    data.Add(JObject.FromObject(indexSettingsDict, _jsonSerializerOptions));
                }
            }

            // Adding Lucene settings
            result.Steps.Add(new JsonObject
            {
                ["name"] = "lucene-index",
                ["Indices"] = data,
            });
        }
    }
}
