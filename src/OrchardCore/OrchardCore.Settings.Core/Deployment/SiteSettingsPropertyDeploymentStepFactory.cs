using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentStepFactory<TModel> : IDeploymentStepFactory
        where TModel : class, new()
    {
        private static readonly string _genericTypeKey = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";

        public string Name => _genericTypeKey;

        public DeploymentStep Create()
        {
            return new SiteSettingsPropertyDeploymentStep<TModel>();
        }
    }
}
