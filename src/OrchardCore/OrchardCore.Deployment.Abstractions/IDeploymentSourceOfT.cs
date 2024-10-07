namespace OrchardCore.Deployment;

/// <summary>
/// Contract for a deployment source.
/// </summary>
/// <typeparam name="TDeploymentStep">The deployment step type.</typeparam>
public interface IDeploymentSource<TDeploymentStep>
    : IDeploymentSource where TDeploymentStep : DeploymentStep
{
    /// <summary>
    /// Gets the deployment step
    /// </summary>
    TDeploymentStep DeploymentStep { get; }
}
