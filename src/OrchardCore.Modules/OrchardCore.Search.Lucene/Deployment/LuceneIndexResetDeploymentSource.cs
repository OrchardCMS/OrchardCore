using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexResetDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexResetStep = step as LuceneIndexResetDeploymentStep;

            if (luceneIndexResetStep == null)
            {
                return Task.CompletedTask;
            }

            var indicesToReset = luceneIndexResetStep.IncludeAll ? Array.Empty<string>() : luceneIndexResetStep.IndexNames;

            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index-reset"),
                new JProperty("includeAll", luceneIndexResetStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToReset))
            ));

            return Task.CompletedTask;
        }
    }
}
