namespace OrchardCore.DataSources;

/// <summary>
/// All the rows read from a data set, up to a limit. Use it when a consumer needs the rows in memory, such as to
/// show a preview.
/// </summary>
public sealed class DataRowSet
{
    /// <summary>
    /// Gets or sets the fields of the rows, aligned by index with each row.
    /// </summary>
    public IReadOnlyList<DataField> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the rows read.
    /// </summary>
    public IList<object[]> Rows { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the data set had more rows than the limit.
    /// </summary>
    public bool Truncated { get; set; }
}
