using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Core.Services
{
    public class DeploymentManager : IDeploymentManager
    {
        private readonly IEnumerable<IDeploymentSource> _deploymentSources;
        private readonly IEnumerable<IDeploymentTargetProvider> _deploymentTargetProviders;
        private readonly IEnumerable<IDeploymentTargetHandler> _deploymentTargetHandlers;

        public DeploymentManager(
            IEnumerable<IDeploymentSource> deploymentSources,
            IEnumerable<IDeploymentTargetProvider> deploymentTargetProviders,
            IEnumerable<IDeploymentTargetHandler> deploymentTargetHandlers)
        {
            _deploymentSources = deploymentSources;
            _deploymentTargetProviders = deploymentTargetProviders;
            _deploymentTargetHandlers = deploymentTargetHandlers;
        }

        public async Task ExecuteDeploymentPlanAsync(DeploymentPlan deploymentPlan, DeploymentPlanResult result)
        {
            foreach (var step in deploymentPlan.DeploymentSteps)
            {
                foreach (var source in _deploymentSources)
                {
                    await source.ProcessDeploymentStepAsync(step, result);
                }
            }

            await result.FinalizeAsync();
        }

        public async Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            var tasks = new List<DeploymentTarget>();

            foreach (var provider in _deploymentTargetProviders)
            {
                tasks.AddRange(await provider.GetDeploymentTargetsAsync());
            }

            return tasks;
        }

        public async Task ImportDeploymentPackageAsync(IFileProvider deploymentPackage)
        {
            foreach (var deploymentTargetHandler in _deploymentTargetHandlers)
            {
                // Don't trigger in parallel to avoid potential race conditions in the handlers
                await deploymentTargetHandler.ImportFromFileAsync(deploymentPackage);
            }
        }
    }
}
