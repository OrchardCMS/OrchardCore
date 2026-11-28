namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    public string Id { get; set; }

    /// <summary>
    /// The name of the step, persisted with the deployment plan. Override its getter to return the name of the step.
    /// </summary>
    /// <remarks>
    /// The setter restores the name of a step whose type isn't available, such as <see cref="UnknownDeploymentStep"/>.
    /// </remarks>
    public virtual string Name { get; set; }
}
