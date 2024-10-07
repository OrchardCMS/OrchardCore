namespace OrchardCore.Deployment;

/// <summary>
/// Represents a base class for deployment source.
/// </summary>
/// <typeparam name="TDeploymentStep">The deployment step type.</typeparam>
public abstract class DeploymentSourceBase<TDeploymentStep>
    : IDeploymentSource where TDeploymentStep : DeploymentStep
{
    public TDeploymentStep DeploymentStep { get; private set; }

    /// <inheritdoc/>
    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not TDeploymentStep)
        {
            return;
        }

        DeploymentStep = step as TDeploymentStep;

        await ProcessDeploymentStepAsync(result);
    }

    public abstract Task ProcessDeploymentStepAsync(DeploymentPlanResult result);
}
