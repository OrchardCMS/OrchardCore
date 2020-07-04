using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    /// <summary>
    /// Adds a site setting from the properties dictionary to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class SiteSettingsPropertyDeploymentStep<TModel> : DeploymentStep where TModel : class, new()
    {
        public SiteSettingsPropertyDeploymentStep()
        {
            Name = "SiteSettingsPropertyDeploymentStep_" + typeof(TModel).Name;
        }
    }
}
