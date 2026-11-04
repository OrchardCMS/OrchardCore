using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Placements.Deployment;

/// <summary>
/// Adds placements to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class PlacementsDeploymentStep : DeploymentStep
{
    public override string Name => "Placements";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Development;

    /// <summary>
    /// Gets or sets the exact names of the shape types to export. All of them are exported when it is <see langword="null"/> or empty.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Names { get; set; }
}
