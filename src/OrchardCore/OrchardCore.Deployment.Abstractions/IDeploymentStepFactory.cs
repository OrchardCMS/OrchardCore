namespace OrchardCore.Deployment
{
    public interface IDeploymentStepFactory
    {
        string Name { get; }
        DeploymentStep Create();
    }

    public class DeploymentStepFactory<TStep> : IDeploymentStepFactory where TStep : DeploymentStep, new()
    {
        private static readonly string TypeName = typeof(TStep).Name;

        public string Name => TypeName;

        public DeploymentStep Create()
        {
            return new TStep();
        }
    }
}
