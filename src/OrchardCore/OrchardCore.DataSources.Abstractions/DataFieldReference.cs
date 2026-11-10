namespace OrchardCore.DataSources;

/// <summary>
/// Says that the values of a field identify records of another data set, such as an order's customer picker holding
/// the id of a customer. Consumers use references to suggest and join related data sets.
/// </summary>
public sealed class DataFieldReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFieldReference"/> class.
    /// </summary>
    public DataFieldReference()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFieldReference"/> class.
    /// </summary>
    /// <param name="source">The technical name of the data source of the referenced data set.</param>
    /// <param name="dataSet">The technical name of the referenced data set.</param>
    /// <param name="field">The technical name of the referenced data set's field that holds the matching values.</param>
    public DataFieldReference(string source, string dataSet, string field)
    {
        Source = source;
        DataSet = dataSet;
        Field = field;
    }

    /// <summary>
    /// Gets or sets the technical name of the data source of the referenced data set.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the referenced data set.
    /// </summary>
    public string DataSet { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the referenced data set's field that holds the matching values, usually its
    /// identifier.
    /// </summary>
    public string Field { get; set; }
}
