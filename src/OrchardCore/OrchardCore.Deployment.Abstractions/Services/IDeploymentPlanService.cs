using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Services
{
    public interface IDeploymentPlanService
    {
        Task<bool> DoesUserHavePermissionsAsync();
        Task<bool> DoesUserHaveExportPermissionAsync();
        Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync();
        Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync();
        Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames);
        void CreateOrUpdateDeploymentPlans(IEnumerable<DeploymentPlan> deploymentPlans);
    }
}
