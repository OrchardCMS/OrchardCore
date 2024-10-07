using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentSource
    : DeploymentSourceBase<AzureAISearchIndexResetDeploymentStep>
{
    public const string Name = "azureai-index-reset";

    protected override Task ProcessAsync(DeploymentPlanResult result)
    {
        var indicesToReset = DeploymentStep.IncludeAll
            ? []
            : DeploymentStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = Name,
            ["includeAll"] = DeploymentStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
