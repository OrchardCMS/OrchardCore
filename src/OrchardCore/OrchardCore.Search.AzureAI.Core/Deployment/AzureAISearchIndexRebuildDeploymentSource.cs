using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentSource
    : DeploymentSourceBase<AzureAISearchIndexRebuildDeploymentStep>
{
    public const string Name = "azureai-index-rebuild";

    protected override Task ProcessAsync(DeploymentPlanResult result)
    {
        var indicesToRebuild = DeploymentStep.IncludeAll
            ? []
            : DeploymentStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = Name,
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
