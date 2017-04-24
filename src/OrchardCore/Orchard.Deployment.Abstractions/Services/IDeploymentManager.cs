using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Deployment.Services
{
    public interface IDeploymentManager
    {
        Task ExecuteDeploymentPlanAsync(DeploymentPlan deploymentPlan, DeploymentPlanResult result);

        Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync();
    }
}
