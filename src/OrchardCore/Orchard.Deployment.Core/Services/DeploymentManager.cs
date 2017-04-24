using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.Deployment.Services;

namespace Orchard.Deployment.Core.Services
{
    public class DeploymentManager : IDeploymentManager
    {
        private readonly IEnumerable<IDeploymentSource> _deploymentSources;
        private readonly IEnumerable<IDeploymentTargetProvider> _deploymentTargetProviders;

        public DeploymentManager(
            IEnumerable<IDeploymentSource> deploymentSources,
            IEnumerable<IDeploymentTargetProvider> deploymentTargetProviders)
        {
            _deploymentSources = deploymentSources;
            _deploymentTargetProviders = deploymentTargetProviders;
        }

        public async Task ExecuteDeploymentPlanAsync(DeploymentPlan deploymentPlan, DeploymentPlanResult result)
        {
            foreach(var step in deploymentPlan.DeploymentSteps)
            {
                foreach(var source in _deploymentSources)
                {
                    await source.ProcessDeploymentStepAsync(step, result);
                }
            }

            await result.FinalizeAsync();
        }

        public async Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            var tasks = new List<DeploymentTarget>();

            foreach(var provider in _deploymentTargetProviders)
            {
                tasks.AddRange(await provider.GetDeploymentTargetsAsync());
            }

            return tasks;
        }
    }
}
