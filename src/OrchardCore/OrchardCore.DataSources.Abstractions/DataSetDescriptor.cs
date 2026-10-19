namespace OrchardCore.DataSources;

/// <summary>
/// Describes one data set a data source exposes, such as a content type, a saved query, or the users of the site.
/// </summary>
public sealed class DataSetDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSetDescriptor"/> class.
    /// </summary>
    public DataSetDescriptor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSetDescriptor"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the data set.</param>
    /// <param name="displayName">The label shown to users.</param>
    /// <param name="description">The optional description shown to users.</param>
    public DataSetDescriptor(string name, string displayName, string description = null)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets or sets the stable technical name of the data set. Saved definitions store this name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the label shown to users.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the optional description shown to users.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the optional group the data set is listed under.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// Gets or sets the name of the data set's main date field, such as when a record was created, if it has one.
    /// </summary>
    public string DefaultDateField { get; set; }

    /// <summary>
    /// Gets or sets the data sets this data set links to, when the data source can tell without reading data.
    /// </summary>
    public IList<DataFieldReference> References { get; set; } = [];
}
