using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    /// <summary>
    /// Adds a generic site settings to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class SiteSettingsPropertyDeploymentStep<TModel> : DeploymentStep where TModel : class
    {
        public SiteSettingsPropertyDeploymentStep()
        {
        }

        public SiteSettingsPropertyDeploymentStep(string title, string description)
        {
            Name = typeof(TModel).Name;
            Title = title;
            Description = description;
        }

        public string Title { get; set; }
        public string Description { get; set; }
    }
}
