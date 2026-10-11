using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// The outcome of a run executed by <see cref="DataPipelineExecutor"/>.
/// </summary>
public sealed class DataPipelineExecutionResult
{
    /// <summary>
    /// Gets or sets how the run ended.
    /// </summary>
    public DataPipelineRunStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the reason the run failed.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the step that failed, if any.
    /// </summary>
    public string FailedStepId { get; set; }

    /// <summary>
    /// Gets or sets the issues the pipeline has.
    /// </summary>
    public IReadOnlyList<DataPipelineIssue> Issues { get; set; } = [];

    /// <summary>
    /// Gets or sets how each step ended, by step identifier.
    /// </summary>
    public IReadOnlyDictionary<string, DataPipelineStepResult> Steps { get; set; } = new Dictionary<string, DataPipelineStepResult>();

    /// <summary>
    /// Gets or sets what the run delivered.
    /// </summary>
    public IReadOnlyList<DataPipelineDelivery> Deliveries { get; set; } = [];
}

/// <summary>
/// How one step ended.
/// </summary>
public sealed class DataPipelineStepResult
{
    /// <summary>
    /// Gets or sets how the step ended.
    /// </summary>
    public DataPipelineStepStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the counters of the step.
    /// </summary>
    public DataPipelineStepMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the reason the step failed.
    /// </summary>
    public string Error { get; set; }
}

/// <summary>
/// What a step produces, or receives, when the pipeline is previewed.
/// </summary>
public sealed class DataPipelinePreview
{
    /// <summary>
    /// Gets or sets the previewed step.
    /// </summary>
    public string StepId { get; set; }

    /// <summary>
    /// Gets or sets why the step can't be previewed, if it can't.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the issues of the pipeline.
    /// </summary>
    public IReadOnlyList<DataPipelineIssue> Issues { get; set; } = [];

    /// <summary>
    /// Gets or sets the data of each previewed port: the outputs of the step, or the inputs of a destination, which a
    /// preview never executes.
    /// </summary>
    public IList<DataPipelinePreviewPort> Ports { get; set; } = [];
}

/// <summary>
/// The data that flowed through one port during a preview.
/// </summary>
public sealed class DataPipelinePreviewPort
{
    /// <summary>
    /// Gets or sets the name of the port.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the label of the port.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets what flows through the port.
    /// </summary>
    public DataPipelinePortKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the fields of the rows.
    /// </summary>
    public IReadOnlyList<DataField> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the first rows.
    /// </summary>
    public List<object[]> Rows { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether more rows flowed than were kept.
    /// </summary>
    public bool Truncated { get; set; }

    /// <summary>
    /// Gets or sets the files.
    /// </summary>
    public List<DataPipelinePreviewFile> Files { get; set; } = [];
}

/// <summary>
/// A file produced during a preview.
/// </summary>
public sealed class DataPipelinePreviewFile
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the media type of the file.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the file, in bytes.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Gets or sets the number of rows in the file, when known.
    /// </summary>
    public long? RowCount { get; set; }
}
