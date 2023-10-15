using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

            result.AddStep("lucene-index-rebuild", new[]
            {
                new KeyValuePair<string, JsonNode>("includeAll", JsonSerializer.SerializeToNode(luceneIndexRebuildStep.IncludeAll)),
                new KeyValuePair<string, JsonNode>("Indices", JsonSerializer.SerializeToNode(indicesToRebuild)),
            });

            return Task.CompletedTask;
        }
    }
}
