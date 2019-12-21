using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneIndexDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneIndexDeploymentSource(LuceneIndexSettingsService luceneIndexSettingsService, ISiteService siteService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexStep = step as LuceneIndexDeploymentStep;

            if (luceneIndexStep == null)
            {
                return;
            }

            var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();
            var indices = luceneIndexStep.IncludeAll ? indexSettings.Select(x => x.IndexName) : luceneIndexStep.IndexNames;

            // Adding Lucene settings
            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index"),
                new JProperty("Indices", JArray.FromObject(indices))
            ));
        }
    }
}
