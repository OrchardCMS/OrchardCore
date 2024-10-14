using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentSource
    : DeploymentSourceBase<AzureAISearchIndexRebuildDeploymentStep>
{
    public const string Name = "azureai-index-rebuild";

    protected override Task ProcessAsync(AzureAISearchIndexRebuildDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll
            ? []
            : step.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = Name,
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
