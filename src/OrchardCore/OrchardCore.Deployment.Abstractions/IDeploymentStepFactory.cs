namespace OrchardCore.Deployment;

public interface IDeploymentStepFactory
{
    string Name { get; }
    DeploymentStep Create();
}
