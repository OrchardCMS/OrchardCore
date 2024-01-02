using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentSource : IDeploymentSource
{
    public const string Name = "azureai-index-rebuild";

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureAISearchIndexRebuildDeploymentStep rebuildStep)
        {
            return Task.CompletedTask;
        }

        var indicesToRebuild = rebuildStep.IncludeAll ? [] : rebuildStep.Indices;

        result.Steps.Add(new JObject(
            new JProperty("name", Name),
            new JProperty("includeAll", rebuildStep.IncludeAll),
            new JProperty("Indices", new JArray(indicesToRebuild))
        ));

        return Task.CompletedTask;
    }
}
