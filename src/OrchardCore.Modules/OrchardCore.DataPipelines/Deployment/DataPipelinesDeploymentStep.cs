using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports data pipelines, every one or the selected ones, with their published definition, or their draft when they
/// were never published, and without the secrets of their steps.
/// </summary>
public class DataPipelinesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<DataPipelinesDeploymentStep>("Data");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<DataPipelinesDeploymentStep>("Data Pipelines");

    public DataPipelinesDeploymentStep()
    {
        Name = "DataPipelines";
        Category = s_category;
        Title = s_title;
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
