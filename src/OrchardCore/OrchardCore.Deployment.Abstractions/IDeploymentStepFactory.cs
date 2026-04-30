using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Deployment;

public interface IDeploymentStepFactory
{
    string Name { get; }
    DeploymentStep Create();
}

public class DeploymentStepFactory<TStep> : IDeploymentStepFactory where TStep : DeploymentStep
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly string _typeName = typeof(TStep).Name;

    public DeploymentStepFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string Name => _typeName;

    public DeploymentStep Create()
    {
        return ActivatorUtilities.CreateInstance<TStep>(_serviceProvider);
    }
}
