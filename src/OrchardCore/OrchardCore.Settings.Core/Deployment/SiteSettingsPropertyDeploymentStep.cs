using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds a site setting from the properties dictionary to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsPropertyDeploymentStep<TModel> : DeploymentStep where TModel : class, new()
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Configuration;

    public SiteSettingsPropertyDeploymentStep()
    {
        Name = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";
    }
}
