using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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

            result.Steps.Add(JObject.Parse(jsonRecipeStep.Json));

            return Task.CompletedTask;
        }
    }
}
