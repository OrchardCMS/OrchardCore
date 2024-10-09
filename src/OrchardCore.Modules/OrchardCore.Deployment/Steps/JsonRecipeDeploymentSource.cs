using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Steps;

public class JsonRecipeDeploymentSource
    : DeploymentSourceBase<JsonRecipeDeploymentStep>
{
    protected override async Task ProcessAsync(JsonRecipeDeploymentStep step, DeploymentPlanResult result)
    {
        result.Steps.Add(JObject.Parse(step.Json));

        await Task.CompletedTask;
    }
}
