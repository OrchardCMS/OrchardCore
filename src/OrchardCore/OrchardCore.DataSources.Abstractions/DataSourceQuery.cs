namespace OrchardCore.DataSources;

/// <summary>
/// Describes the rows a consumer asks a data source to read from one data set.
/// </summary>
public sealed class DataSourceQuery
{
    /// <summary>
    /// The number of rows a data source returns per batch when <see cref="BatchSize"/> is not set.
    /// </summary>
    public const int DefaultBatchSize = 500;

    /// <summary>
    /// Gets or sets the technical name of the data set to read.
    /// </summary>
    public string DataSet { get; set; }

    /// <summary>
    /// Gets or sets the technical names of the fields the consumer uses. A data source returns at least these fields
    /// and may skip the work of computing any other field. When empty, every field is needed.
    /// </summary>
    public ISet<string> Fields { get; set; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the filter conditions the source may apply while reading. See <see cref="DataCondition"/>.
    /// </summary>
    public IList<DataCondition> Conditions { get; set; } = [];

    /// <summary>
    /// Gets or sets the most rows the source may return, or <c>0</c> to read every row.
    /// </summary>
    public int MaxRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows the source returns per batch. Sources read their store page by page, so a
    /// consumer can process any number of rows without holding them all in memory.
    /// </summary>
    public int BatchSize { get; set; } = DefaultBatchSize;

    /// <summary>
    /// Gets or sets the values of the parameters of a data set that takes them, such as a saved query.
    /// </summary>
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the caller context of the run.
    /// </summary>
    public DataSourceContext Context { get; set; } = new();
}
