namespace OrchardCore.DataSources;

/// <summary>
/// A batch of rows read from a data set or produced by a step that transforms rows. Each row holds one value per entry
/// of <see cref="Fields"/>, in the same order, using the CLR type documented on <see cref="DataFieldType"/>.
/// </summary>
public sealed class DataBatch
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataBatch"/> class.
    /// </summary>
    /// <param name="fields">The fields of the rows.</param>
    /// <param name="rows">The rows, aligned by index with <paramref name="fields"/>.</param>
    public DataBatch(IReadOnlyList<DataField> fields, IReadOnlyList<object[]> rows)
    {
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(rows);

        Fields = fields;
        Rows = rows;
    }

    /// <summary>
    /// Gets the fields of the rows, aligned by index with each row.
    /// </summary>
    public IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Gets the rows of the batch.
    /// </summary>
    public IReadOnlyList<object[]> Rows { get; }

    /// <summary>
    /// Gets the number of rows in the batch.
    /// </summary>
    public int Count => Rows.Count;

    /// <summary>
    /// Finds the index of a field by its technical name.
    /// </summary>
    /// <param name="name">The technical field name.</param>
    /// <returns>The zero-based index, or <c>-1</c> when the batch has no such field.</returns>
    public int IndexOf(string name) => Fields.IndexOf(name);
}
