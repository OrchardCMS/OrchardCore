using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class DeleteContentDefinitionDeploymentSource : IDeploymentSource
{
    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not DeleteContentDefinitionDeploymentStep deleteContentDefinitionStep)
        {
            return Task.CompletedTask;
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "DeleteContentDefinition",
            ["ContentTypes"] = JArray.FromObject(deleteContentDefinitionStep.ContentTypes),
            ["ContentParts"] = JArray.FromObject(deleteContentDefinitionStep.ContentParts),
        });

        return Task.CompletedTask;
    }
}
