using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexResetDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not LuceneIndexResetDeploymentStep luceneIndexResetStep)
            {
                return Task.CompletedTask;
            }

            var indicesToReset = luceneIndexResetStep.IncludeAll ? [] : luceneIndexResetStep.IndexNames;

            result.Steps.Add(new JsonObject
            {
                ["name"] = "lucene-index-reset",
                ["includeAll"] = luceneIndexResetStep.IncludeAll,
                ["Indices"] = JArray.FromObject(indicesToReset),
            });

            return Task.CompletedTask;
        }
    }
}
