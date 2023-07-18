using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Deployment.Deployment
{
    public class DeploymentPlanDeploymentSource : IDeploymentSource
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly IEnumerable<IDeploymentStepFactory> _deploymentStepFactories;

        public DeploymentPlanDeploymentSource(
            IDeploymentPlanService deploymentPlanService,
            IEnumerable<IDeploymentStepFactory> deploymentStepFactories)
        {
            _deploymentPlanService = deploymentPlanService;
            _deploymentStepFactories = deploymentStepFactories;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
        {
            if (deploymentStep is not DeploymentPlanDeploymentStep deploymentPlanStep)
            {
                return;
            }

            if (!await _deploymentPlanService.DoesUserHavePermissionsAsync())
            {
                return;
            }

            var deploymentStepFactories = _deploymentStepFactories.ToDictionary(f => f.Name);

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
                                          Type = GetStepType(deploymentStepFactories, step),
                                          Step = step
                                      }).ToArray(),
                         }).ToArray();

            // Adding deployment plans.
            result.Steps.Add(new JObject(
                new JProperty("name", "deployment"),
                new JProperty("Plans", JArray.FromObject(plans))
            ));
        }

        /// <summary>
        /// A Site Settings Step is generic and the name is mapped to the <see cref="IDeploymentStepFactory.Name"/> so its 'Type' should be determined though a lookup.
        /// A normal steps name is not mapped to the <see cref="IDeploymentStepFactory.Name"/> and should use its type.
        /// </summary>
        private static string GetStepType(IDictionary<string, IDeploymentStepFactory> deploymentStepFactories, DeploymentStep step)
        {
            if (deploymentStepFactories.TryGetValue(step.Name, out var deploymentStepFactory))
            {
                return deploymentStepFactory.Name;
            }
            else
            {
                return step.GetType().Name;
            }
        }
    }
}
