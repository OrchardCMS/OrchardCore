using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Layers.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllLayersDeploymentStep : DeploymentStep
{
    public override string Name => "AllLayers";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Content;
}
