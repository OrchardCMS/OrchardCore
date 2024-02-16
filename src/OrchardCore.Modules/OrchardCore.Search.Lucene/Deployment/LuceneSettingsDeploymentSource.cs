using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneSettingsDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public LuceneSettingsDeploymentSource(
            LuceneIndexingService luceneIndexingService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _luceneIndexingService = luceneIndexingService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneSettingsStep = step as LuceneSettingsDeploymentStep;

            if (luceneSettingsStep == null)
            {
                return;
            }

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            // Adding Lucene settings
            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                ["LuceneSettings"] = JObject.FromObject(luceneSettings, _jsonSerializerOptions),
            });
        }
    }
}
