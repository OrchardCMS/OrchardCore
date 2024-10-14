using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Steps;

public class JsonRecipeDeploymentSource
    : DeploymentSourceBase<JsonRecipeDeploymentStep>
{
    protected override Task ProcessAsync(JsonRecipeDeploymentStep step, DeploymentPlanResult result)
    {
        result.Steps.Add(JObject.Parse(step.Json));

        return Task.CompletedTask;
    }
}
