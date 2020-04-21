using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Deployment.Deployment
{
    public class DeploymentPlanDeploymentSource : IDeploymentSource
    {
        private readonly DeploymentPlanService _deploymentPlanService;

        public DeploymentPlanDeploymentSource(DeploymentPlanService deploymentPlanService)
        {
            _deploymentPlanService = deploymentPlanService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
        {
            if (!(deploymentStep is DeploymentPlanDeploymentStep deploymentPlanStep))
            {
                return;
            }

            if (!await _deploymentPlanService.DoesUserHavePermissionsAsync())
            {
                return;
            }

            var deploymentPlans = deploymentPlanStep.IncludeAll
                ? (await _deploymentPlanService.GetAllDeploymentPlansAsync()).ToArray()
                : (await _deploymentPlanService.GetDeploymentPlansAsync(deploymentPlanStep.DeploymentPlanNames)).ToArray();

            var plans = (from plan in deploymentPlans
                         select new
                         {
                             plan.Name,
                             Steps = (from step in plan.DeploymentSteps
                                      select new
                                      {
                                          Type = step.GetType().Name,
                                          Step = step
                                      }).ToArray()
                         }).ToArray();

            // Adding deployment plans
            result.Steps.Add(new JObject(
                new JProperty("name", "deployment"),
                new JProperty("Plans", JArray.FromObject(plans))
            ));
        }
    }
}
