using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Deployment.Services
{
    public interface IDeploymentManager
    {
        Task ExecuteDeploymentPlanAsync(DeploymentPlan deploymentPlan, DeploymentPlanResult result);
        Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync();
        Task ImportDeploymentPackageAsync(IFileProvider deploymentPackage);
    }
}
