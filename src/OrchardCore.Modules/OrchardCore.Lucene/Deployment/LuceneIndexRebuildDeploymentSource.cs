using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneIndexRebuildDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneIndexRebuildDeploymentSource(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexRebuildStep = step as LuceneIndexRebuildDeploymentStep;

            if (luceneIndexRebuildStep == null)
            {
                return;
            }

            var indexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

            var data = new JArray();
            var indicesToRebuild = luceneIndexRebuildStep.IncludeAll ? indexSettings.Select(x => x.IndexName).ToArray() : luceneIndexRebuildStep.IndexNames;

            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index-rebuild"),
                new JProperty("Indices", new JArray(indicesToRebuild))
            ));
        }
    }
}
