namespace OrchardCore.Deployment;

public interface IDeploymentTargetProvider
{
    Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync();
}
