using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexRebuildDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not LuceneIndexRebuildDeploymentStep luceneIndexRebuildStep)
            {
                return Task.CompletedTask;
            }

            var indicesToRebuild = luceneIndexRebuildStep.IncludeAll ? [] : luceneIndexRebuildStep.IndexNames;

            result.Steps.Add(new JsonObject
            {
                ["name"] = "lucene-index-rebuild",
                ["includeAll"] = luceneIndexRebuildStep.IncludeAll,
                ["Indices"] = JArray.FromObject(indicesToRebuild),
            });

            return Task.CompletedTask;
        }
    }
}
