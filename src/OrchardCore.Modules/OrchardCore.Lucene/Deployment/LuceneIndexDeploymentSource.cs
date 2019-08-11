using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneIndexDeploymentSource : IDeploymentSource
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly ISiteService _siteService;

        public LuceneIndexDeploymentSource(LuceneIndexManager luceneIndexManager, ISiteService siteService)
        {
            _luceneIndexManager = luceneIndexManager;
            _siteService = siteService;
        }

        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexStep = step as LuceneIndexDeploymentStep;

            if (luceneIndexStep == null)
            {
                return Task.CompletedTask;
            }

            var indices = luceneIndexStep.IncludeAll ? _luceneIndexManager.List().ToArray() : luceneIndexStep.IndexNames;

            // Adding Lucene settings
            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index"),
                new JProperty("Indices", JArray.FromObject(indices))
            ));

            return Task.CompletedTask;
        }
    }
}
