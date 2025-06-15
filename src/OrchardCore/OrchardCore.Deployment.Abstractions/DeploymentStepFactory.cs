namespace OrchardCore.Deployment;

public class DeploymentStepFactory<TStep> : IDeploymentStepFactory where TStep : DeploymentStep, new()
{
    private static readonly string _typeName = typeof(TStep).Name;

    public string Name => _typeName;

    public DeploymentStep Create()
    {
        return new TStep();
    }
}
