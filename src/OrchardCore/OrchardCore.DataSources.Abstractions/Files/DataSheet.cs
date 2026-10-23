namespace OrchardCore.DataSources.Files;

/// <summary>
/// One data set written by an <see cref="IDataWorkbookFormat"/> to its own worksheet.
/// </summary>
public sealed class DataSheet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSheet"/> class.
    /// </summary>
    /// <param name="name">
    /// The name of the worksheet. A name that the format can't use as is, such as an empty or a repeated name, is
    /// adjusted to a valid and unique one.
    /// </param>
    /// <param name="fields">The fields of the rows, which become the columns of the worksheet.</param>
    /// <param name="batches">The batches of rows to write. Every batch must have the same fields as <paramref name="fields"/>.</param>
    public DataSheet(string name, IReadOnlyList<DataField> fields, IAsyncEnumerable<DataBatch> batches)
    {
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(batches);

        Name = name;
        Fields = fields;
        Batches = batches;
    }

    /// <summary>
    /// Gets the name of the worksheet.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the fields of the rows, which become the columns of the worksheet.
    /// </summary>
    public IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Gets the batches of rows to write.
    /// </summary>
    public IAsyncEnumerable<DataBatch> Batches { get; }
}
