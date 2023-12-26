using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        result.Steps.Add(new JObject(
        new JProperty("name", Name),
            new JProperty("includeAll", resetStep.IncludeAll),
            new JProperty("Indices", new JArray(indicesToReset))
        ));

        return Task.CompletedTask;
    }
}
