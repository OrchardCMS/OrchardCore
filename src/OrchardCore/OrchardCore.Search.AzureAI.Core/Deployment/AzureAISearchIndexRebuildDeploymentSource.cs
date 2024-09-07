using System.Text.Json.Nodes;
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

        result.Steps.Add(new JsonObject
        {
            ["name"] = Name,
            ["includeAll"] = rebuildStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
