using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class DeleteContentDefinitionDeploymentSource
    : DeploymentSourceBase<DeleteContentDefinitionDeploymentStep>
{
    protected override async Task ProcessAsync(DeleteContentDefinitionDeploymentStep step, DeploymentPlanResult result)
    {
        result.Steps.Add(new JsonObject
        {
            ["name"] = "DeleteContentDefinition",
            ["ContentTypes"] = JArray.FromObject(step.ContentTypes),
            ["ContentParts"] = JArray.FromObject(step.ContentParts),
        });

        await Task.CompletedTask;
    }
}
