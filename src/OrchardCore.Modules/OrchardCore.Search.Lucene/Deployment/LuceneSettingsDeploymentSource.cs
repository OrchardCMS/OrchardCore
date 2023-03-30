using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneSettingsDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexingService _luceneIndexingService;

        public LuceneSettingsDeploymentSource(LuceneIndexingService luceneIndexingService)
        {
            _luceneIndexingService = luceneIndexingService;
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
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LuceneSettings", JObject.FromObject(luceneSettings))
            ));
        }
    }
}
