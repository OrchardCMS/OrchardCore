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

    /// <summary>
    /// Gets or sets the exact names of the layers. With names, the layer settings of the site are left out to export. All of them are exported when it is <see langword="null"/> or empty.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Names { get; set; }
}
