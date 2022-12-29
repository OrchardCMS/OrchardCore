using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexRebuildDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var luceneIndexRebuildStep = step as LuceneIndexRebuildDeploymentStep;

            if (luceneIndexRebuildStep == null)
            {
                return Task.CompletedTask;
            }

            var indicesToRebuild = luceneIndexRebuildStep.IncludeAll ? Array.Empty<string>() : luceneIndexRebuildStep.IndexNames;

            result.Steps.Add(new JObject(
                new JProperty("name", "lucene-index-rebuild"),
                new JProperty("includeAll", luceneIndexRebuildStep.IncludeAll),
                new JProperty("Indices", new JArray(indicesToRebuild))
            ));

            return Task.CompletedTask;
        }
    }
}
