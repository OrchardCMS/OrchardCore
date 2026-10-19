using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports data pipelines, every one or the selected ones, with their published definition, or their draft when they
/// were never published, and without the secrets of their steps.
/// </summary>
public class DataPipelinesDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Data;

    [JsonIgnore]
    public override LocalizationSource Title => DataPipelinesLocalizationSources.DataPipelines;

    public DataPipelinesDeploymentStep()
    {
        Name = "DataPipelines";
    }

    /// <summary>
    /// Gets or sets a value indicating whether every pipeline is exported, including the ones created later.
    /// </summary>
    public bool IncludeAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the identifiers of the pipelines to export when <see cref="IncludeAll"/> is <see langword="false"/>.
    /// </summary>
    public string[] PipelineIds { get; set; } = [];
}
