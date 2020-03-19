namespace OrchardCore.Deployment
{
    public interface IOrderedDeploymentSource : IDeploymentSource
    {
        int Order { get; }
    }
}
