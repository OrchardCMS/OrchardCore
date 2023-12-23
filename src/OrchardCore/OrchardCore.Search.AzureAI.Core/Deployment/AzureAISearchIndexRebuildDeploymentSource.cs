using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentSource : IDeploymentSource
{
    public const string Name = "azureai-index-rebuild";

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticIndexRebuildStep = step as AzureAISearchIndexRebuildDeploymentStep;
        if (elasticIndexRebuildStep == null)
        {
            return Task.CompletedTask;
        }

        var indicesToRebuild = elasticIndexRebuildStep.IncludeAll ? [] : elasticIndexRebuildStep.Indices;

        result.Steps.Add(new JObject(
            new JProperty("name", Name),
            new JProperty("includeAll", elasticIndexRebuildStep.IncludeAll),
            new JProperty("Indices", new JArray(indicesToRebuild))
        ));

        return Task.CompletedTask;
    }
}
