using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

/// <summary>
/// Adds a site setting from the properties dictionary to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SiteSettingsPropertyDeploymentStep<TModel> : DeploymentStep where TModel : class, new()
{
    private static readonly string s_name = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";

    public override string Name => s_name;

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Configuration;
}
