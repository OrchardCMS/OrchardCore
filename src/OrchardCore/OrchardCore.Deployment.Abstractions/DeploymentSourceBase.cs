namespace OrchardCore.Deployment;

/// <summary>
/// Represents a base class for deployment source.
/// </summary>
/// <typeparam name="TDeploymentStep">The deployment step type.</typeparam>
public abstract class DeploymentSourceBase<TDeploymentStep>
    : IDeploymentSource where TDeploymentStep : DeploymentStep
{
    protected TDeploymentStep DeploymentStep { get; private set; }

    /// <inheritdoc/>
    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not TDeploymentStep deploymentStep)
        {
            return;
        }

        await ProcessAsync(deploymentStep, result);
    }

    protected abstract Task ProcessAsync(DeploymentStep step, DeploymentPlanResult result);
}
