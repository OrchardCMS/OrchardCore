namespace OrchardCore.Deployment;

/// <summary>
/// Represents a base class for deployment source.
/// </summary>
/// <typeparam name="TStep">The deployment step type.</typeparam>
public abstract class DeploymentSourceBase<TStep> : IDeploymentSource
    where TStep : DeploymentStep
{
    /// <inheritdoc/>
    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not TStep deploymentStep)
        {
            return Task.CompletedTask;
        }

        return ProcessAsync(deploymentStep, result);
    }

    protected abstract Task ProcessAsync(TStep step, DeploymentPlanResult result);
}
