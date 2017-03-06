using System.Threading.Tasks;

namespace Orchard.Deployment
{
    /// <summary>
    /// Interprets steps from a deployment plan to build the result package.
    /// </summary>
    public interface IDeploymentSource
    {
        Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result);
    }
}
