namespace OrchardCore.DataPipelines;

/// <summary>
/// The limits of the data pipeline engine, bound to the <c>OrchardCore:DataPipelines</c> configuration section.
/// </summary>
public sealed class DataPipelineOptions
{
    /// <summary>
    /// The number of rows a step may hold in memory by default.
    /// </summary>
    public const int DefaultMaxRowsInMemory = 1_000_000;

    /// <summary>
    /// Gets or sets the number of rows a step that must see every row before it writes any, such as a sort or the
    /// right input of a join, may hold in memory. A run fails rather than exhausting the memory of the server.
    /// </summary>
    public int MaxRowsInMemory { get; set; } = DefaultMaxRowsInMemory;

    /// <summary>
    /// Gets or sets the number of days the runs of a pipeline are kept. Older runs are deleted. Defaults to 30.
    /// </summary>
    public int RunRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of rows each source reads when a step is previewed. Defaults to 100.
    /// </summary>
    public int PreviewRowLimit { get; set; } = 100;
}
