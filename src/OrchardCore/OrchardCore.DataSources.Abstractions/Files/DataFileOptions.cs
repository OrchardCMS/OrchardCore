namespace OrchardCore.DataSources.Files;

/// <summary>
/// The options used to write or read a data file. Each format uses the options that apply to it.
/// </summary>
public sealed class DataFileOptions
{
    /// <summary>
    /// Gets or sets the character that separates the values of a delimited text file. Defaults to a comma.
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Gets or sets a value indicating whether the first row of a delimited text or spreadsheet file holds the names of
    /// the columns. Defaults to <see langword="true"/>.
    /// </summary>
    public bool HasHeaderRow { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the worksheet of a spreadsheet file. When reading, the first worksheet is used when it
    /// is empty.
    /// </summary>
    public string SheetName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a JSON file is indented.
    /// </summary>
    public bool Indented { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether columns use the label of each field instead of its technical name.
    /// </summary>
    public bool UseDisplayNames { get; set; }

    /// <summary>
    /// Gets or sets the number of rows read per batch. Defaults to <see cref="DataSourceQuery.DefaultBatchSize"/>.
    /// </summary>
    public int BatchSize { get; set; } = DataSourceQuery.DefaultBatchSize;
}
