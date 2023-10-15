using System;
using System.Collections.Generic;
using System.Text.Json;
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

            result.AddStep("lucene-index-rebuild", new[]
            {
                new KeyValuePair<string, JsonNode>("includeAll", JsonSerializer.SerializeToNode(luceneIndexResetStep.IncludeAll)),
                new KeyValuePair<string, JsonNode>("Indices", JsonSerializer.SerializeToNode(indicesToReset)),
            });

            return Task.CompletedTask;
        }
    }
}
