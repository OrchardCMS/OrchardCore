namespace OrchardCore.DataPipelines;

/// <summary>
/// The limits of the data pipeline engine, bound to the <c>OrchardCore:DataPipelines</c> configuration section.
/// </summary>
public sealed class DataPipelineOptions
{
    /// <summary>
    /// The memory a step may use by default, in megabytes.
    /// </summary>
    public const int DefaultStepMemoryLimitMegabytes = 32;

    /// <summary>
    /// The number of runs of a tenant that execute at the same time by default.
    /// </summary>
    public const int DefaultMaxConcurrentRuns = 4;

    /// <summary>
    /// Gets or sets the memory, in megabytes, a step that must see many rows at once may use: the rows of
    /// <b>Sort rows</b>, the right input of <b>Join</b>, the groups of <b>Group and summarize</b>, and the keys of
    /// <b>Remove duplicates</b>. The rows that don't fit are written to temporary files of the run, so a step
    /// handles any number of rows with this much memory. Defaults to 32.
    /// </summary>
    public int StepMemoryLimitMegabytes { get; set; } = DefaultStepMemoryLimitMegabytes;

    /// <summary>
    /// Gets or sets the number of runs of the tenant that execute at the same time. The other runs wait, queued, for
    /// one to end. Defaults to 4.
    /// </summary>
    public int MaxConcurrentRuns { get; set; } = DefaultMaxConcurrentRuns;

    /// <summary>
    /// Gets or sets the number of days the runs of a pipeline are kept. Older runs are deleted; 0 keeps them. Defaults to 30.
    /// </summary>
    public int RunRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of rows each source reads when a step is previewed. Defaults to 100.
    /// </summary>
    public int PreviewRowLimit { get; set; } = 100;
}
