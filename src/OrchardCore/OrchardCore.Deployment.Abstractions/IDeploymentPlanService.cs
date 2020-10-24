using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public interface IDeploymentPlanService
    {
        Task<bool> DoesUserHavePermissionsAsync();
        Task<bool> DoesUserHaveExportPermissionAsync();
        Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync();
        Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync();
        Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames);
        Task CreateOrUpdateDeploymentPlansAsync(IEnumerable<DeploymentPlan> deploymentPlans);
    }
}
