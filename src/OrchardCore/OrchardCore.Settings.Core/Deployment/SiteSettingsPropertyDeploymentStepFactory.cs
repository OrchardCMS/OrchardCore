using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Settings.Deployment;

public class SiteSettingsPropertyDeploymentStepFactory<TModel> : IDeploymentStepFactory
    where TModel : class, new()
{
    private static readonly string s_genericTypeKey = typeof(TModel).Name + "_SiteSettingsPropertyDeploymentStep";

    public string Name => s_genericTypeKey;

    [JsonIgnore]
    public LocalizationSource Category => SiteSettingsPropertyDeploymentStep<TModel>.Category;

    [JsonIgnore]
    public LocalizationSource Title => null;

    public DeploymentStep Create()
    {
        return new SiteSettingsPropertyDeploymentStep<TModel>();
    }
}
