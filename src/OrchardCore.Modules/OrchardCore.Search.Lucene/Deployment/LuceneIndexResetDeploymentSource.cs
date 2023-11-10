using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

            result.Steps.Add(new JsonObject
            {
                ["name"] = "lucene-index-reset",
                ["includeAll"] = luceneIndexResetStep.IncludeAll,
                ["Indices"] = new JsonArray(indicesToReset.Select(i => JsonValue.Create(i)).ToArray()),
            });

            return Task.CompletedTask;
        }
    }
}
