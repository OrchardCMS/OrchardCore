using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports data pipelines, every one or the selected ones, with their published definition, or their draft when they
/// were never published, and without the secrets of their steps.
/// </summary>
public class DataPipelinesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "DataPipelines";

    public static LocalizationSource Category => DataPipelinesLocalizationSources.Data;

    public static LocalizationSource Title => DataPipelinesLocalizationSources.DataPipelines;

    /// <summary>
    /// Gets or sets a value indicating whether every pipeline is exported, including the ones created later.
    /// </summary>
    public bool IncludeAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the identifiers of the pipelines to export when <see cref="IncludeAll"/> is <see langword="false"/>.
    /// </summary>
    public string[] PipelineIds { get; set; } = [];
}
