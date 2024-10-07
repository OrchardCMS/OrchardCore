using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Steps;

public class JsonRecipeDeploymentSource
    : DeploymentSourceBase<JsonRecipeDeploymentStep>
{
    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        result.Steps.Add(JObject.Parse(DeploymentStep.Json));

        await Task.CompletedTask;
    }
}
