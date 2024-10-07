using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class DeleteContentDefinitionDeploymentSource
    : DeploymentSourceBase<DeleteContentDefinitionDeploymentStep>
{
    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        result.Steps.Add(new JsonObject
        {
            ["name"] = "DeleteContentDefinition",
            ["ContentTypes"] = JArray.FromObject(DeploymentStep.ContentTypes),
            ["ContentParts"] = JArray.FromObject(DeploymentStep.ContentParts),
        });

        await Task.CompletedTask;
    }
}
