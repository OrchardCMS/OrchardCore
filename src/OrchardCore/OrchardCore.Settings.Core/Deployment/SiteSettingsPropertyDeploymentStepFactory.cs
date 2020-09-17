using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentStepFactory<TModel> : IDeploymentStepFactory
        where TModel : class, new()
    {
        private static readonly string GenericTypeKey = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";

        public string Name => GenericTypeKey;

        public DeploymentStep Create()
        {
            return new SiteSettingsPropertyDeploymentStep<TModel>();
        }
    }
}
