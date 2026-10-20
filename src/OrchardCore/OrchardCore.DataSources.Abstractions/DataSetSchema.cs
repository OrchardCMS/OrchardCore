namespace OrchardCore.DataSources;

/// <summary>
/// Describes the fields of one data set.
/// </summary>
public sealed class DataSetSchema
{
    /// <summary>
    /// Gets or sets the data set the schema describes.
    /// </summary>
    public DataSetDescriptor DataSet { get; set; }

    /// <summary>
    /// Gets or sets the fields of the data set, in the order they are listed.
    /// </summary>
    public IList<DataField> Fields { get; set; } = [];

    /// <summary>
    /// Finds a field by its technical name.
    /// </summary>
    /// <param name="name">The technical field name.</param>
    /// <returns>The field, or <see langword="null"/> when the data set has no such field.</returns>
    public DataField FindField(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return Fields.FirstOrDefault(field => string.Equals(field.Name, name, StringComparison.Ordinal));
    }
}
