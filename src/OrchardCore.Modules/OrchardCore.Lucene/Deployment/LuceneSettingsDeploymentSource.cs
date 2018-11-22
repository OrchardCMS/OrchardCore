using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneSettingsDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly ISiteService _siteService;

        public LuceneSettingsDeploymentSource(LuceneIndexingService luceneIndexingService, ISiteService siteService)
        {
            _luceneIndexingService = luceneIndexingService;
            _siteService = siteService;
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
