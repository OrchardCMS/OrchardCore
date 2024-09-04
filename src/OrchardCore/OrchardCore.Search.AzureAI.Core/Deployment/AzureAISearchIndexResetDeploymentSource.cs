using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentSource : IDeploymentSource
{
    public const string Name = "azureai-index-reset";

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureAISearchIndexResetDeploymentStep resetStep)
        {
            return Task.CompletedTask;
        }

        var indicesToReset = resetStep.IncludeAll ? [] : resetStep.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = Name,
            ["includeAll"] = resetStep.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
