using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentSource : IDeploymentSource
{
    public const string Name = "azureai-index-reset";

    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var elasticIndexResetStep = step as AzureAISearchIndexResetDeploymentStep;

        if (elasticIndexResetStep == null)
        {
            return Task.CompletedTask;
        }

        var indicesToReset = elasticIndexResetStep.IncludeAll ? [] : elasticIndexResetStep.Indices;

        result.Steps.Add(new JObject(
        new JProperty("name", Name),
            new JProperty("includeAll", elasticIndexResetStep.IncludeAll),
            new JProperty("Indices", new JArray(indicesToReset))
        ));

        return Task.CompletedTask;
    }
}
