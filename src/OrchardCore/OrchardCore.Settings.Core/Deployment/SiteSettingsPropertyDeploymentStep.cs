using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds a site setting from the properties dictionary to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsPropertyDeploymentStep<TModel> : DeploymentStep where TModel : class, new()
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<SiteSettingsPropertyDeploymentStep<TModel>>("Configuration");

    public SiteSettingsPropertyDeploymentStep()
    {
        Name = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";
        Category = s_category;
    }
}
