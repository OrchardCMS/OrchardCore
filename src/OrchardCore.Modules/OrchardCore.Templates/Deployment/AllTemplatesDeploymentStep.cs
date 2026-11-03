using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep
{
    public override string Name => "AllTemplates";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Development;

    [JsonIgnore]
    public override LocalizationSource Title => TemplatesLocalizationSources.AllTemplates;

    public bool ExportAsFiles { get; set; }

    /// <summary>
    /// Gets or sets the exact names of the templates to export. All of them are exported when it is <see langword="null"/> or empty.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Names { get; set; }
}
