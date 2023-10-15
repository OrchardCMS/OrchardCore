using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Steps
{
    public class JsonRecipeDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
        {
            if (deploymentStep is not JsonRecipeDeploymentStep jsonRecipeStep)
            {
                return Task.CompletedTask;
            }

            result.Steps.Add(JsonNode.Parse(jsonRecipeStep.Json)!.AsObject());

            return Task.CompletedTask;
        }
    }
}
