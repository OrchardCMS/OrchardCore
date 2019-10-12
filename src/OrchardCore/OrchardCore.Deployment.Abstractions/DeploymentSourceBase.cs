using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public abstract class DeploymentSourceBase : IDeploymentSource
    {
        /// <inheritdoc />
        public virtual int Order { get; } = 0;

        /// <inheritdoc />
        public abstract Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result);
    }
}