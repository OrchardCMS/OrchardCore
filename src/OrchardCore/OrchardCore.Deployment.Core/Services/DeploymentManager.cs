using System.Collections.Generic;
using System.Linq;
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
            var orderedDeploymentSources = _deploymentSources
                .Where(s => typeof(IOrderedDeploymentSource).IsAssignableFrom(s.GetType()))
                .Select(s => (IOrderedDeploymentSource)s)
                .OrderBy(s => s.Order)
                .ToList();
            var unorderedDeploymentSources = _deploymentSources
                .Except(orderedDeploymentSources)
                .Union(orderedDeploymentSources.Where(s => s.Order == 0));

            await ExecuteDeploymentPlanInternalAsync(orderedDeploymentSources.Where(s => s.Order < 0), deploymentPlan, result);
            await ExecuteDeploymentPlanInternalAsync(unorderedDeploymentSources, deploymentPlan, result);
            await ExecuteDeploymentPlanInternalAsync(orderedDeploymentSources.Where(s => s.Order > 0), deploymentPlan, result);

            await result.FinalizeAsync();

            async Task ExecuteDeploymentPlanInternalAsync(IEnumerable<IDeploymentSource> deploymentSources, DeploymentPlan deploymentPlan, DeploymentPlanResult result)
            {
                foreach (var source in deploymentSources)
                {
                    foreach (var step in deploymentPlan.DeploymentSteps)
                    {
                        await source.ProcessDeploymentStepAsync(step, result);
                    }
                }
            }
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

        private async Task ExecuteDeploymentPlanInternalAsync(IEnumerable<IOrderedDeploymentSource> deploymentSources, DeploymentPlan deploymentPlan, DeploymentPlanResult result)
        {
            foreach (var source in deploymentSources)
            {
                foreach (var step in deploymentPlan.DeploymentSteps)
                {
                    await source.ProcessDeploymentStepAsync(step, result);
                }
            }
        }
    }
}
