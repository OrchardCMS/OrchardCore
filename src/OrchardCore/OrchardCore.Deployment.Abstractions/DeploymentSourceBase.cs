namespace OrchardCore.Deployment;

/// <summary>
/// Represents a base class for deployment source.
/// </summary>
/// <typeparam name="TDeploymentStep">The deployment step type.</typeparam>
public abstract class DeploymentSourceBase<TDeploymentStep>
    : IDeploymentSource where TDeploymentStep : DeploymentStep
{
    /// <inheritdoc/>
    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not TDeploymentStep deploymentStep)
        {
            return Task.CompletedTask;
        }

        return ProcessAsync(deploymentStep, result);
    }

    protected abstract Task ProcessAsync(TDeploymentStep step, DeploymentPlanResult result);
}
