using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Deployment.Services
{
    public class DeploymentManager : IDeploymentManager
    {
        private readonly IEnumerable<IDeploymentSource> _deploymentSources;

        public DeploymentManager(IEnumerable<IDeploymentSource> deploymentSources)
        {
            _deploymentSources = deploymentSources;
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
    }
}
