using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Steps;

public class JsonRecipeDeploymentSource
    : DeploymentSourceBase<JsonRecipeDeploymentStep>
{
    protected override async Task ProcessAsync(DeploymentPlanResult result)
    {
        result.Steps.Add(JObject.Parse(DeploymentStep.Json));

        await Task.CompletedTask;
    }
}
