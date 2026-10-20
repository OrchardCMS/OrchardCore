using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Selects the runs of the run history.
/// </summary>
public sealed class DataPipelineRunFilter
{
    /// <summary>
    /// Gets or sets the pipeline whose runs are listed, or <see langword="null"/> for every pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets a text the name of the pipeline of a run must contain, or <see langword="null"/>.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the status of the runs, or <see langword="null"/> for every status.
    /// </summary>
    public DataPipelineRunStatus? Status { get; set; }
}
