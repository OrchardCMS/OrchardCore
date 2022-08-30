using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneIndexResetDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneIndexResetDeploymentSource(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexResetStep = step as LuceneIndexResetDeploymentStep;

            if (luceneIndexResetStep == null)
            {
                return;
            }

            var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

            var data = new JArray();
            var indicesToReset = luceneIndexResetStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : luceneIndexResetStep.IndexNames;

            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index-reset"),
                new JProperty("Indices", new JArray(indicesToReset))
            ));
        }
    }
}
