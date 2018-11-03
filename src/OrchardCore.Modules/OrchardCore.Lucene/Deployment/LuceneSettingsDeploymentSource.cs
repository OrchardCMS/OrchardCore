using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneSettingsDeploymentSource : IDeploymentSource
    {
        private readonly LuceneAnalyzerSettingsService _luceneAnalyzerSettingsService;
        private readonly ISiteService _siteService;

        public LuceneSettingsDeploymentSource(LuceneAnalyzerSettingsService luceneAnalyzerSettingsService, ISiteService siteService)
        {
            _luceneAnalyzerSettingsService = luceneAnalyzerSettingsService;
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneSettingsStep = step as LuceneSettingsDeploymentStep;

            if (luceneSettingsStep == null)
            {
                return;
            }

            var luceneSettings = await _luceneAnalyzerSettingsService.GetLuceneAnalyzerSettingsAsync();

            // Adding Lucene settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LuceneSettings", JObject.FromObject(luceneSettings))
            ));
        }
    }
}
