namespace OrchardCore.DataSources.Contents;

/// <summary>
/// One value of a content part or a content field, read from the JSON property of the same name.
/// </summary>
public sealed class ContentDataValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDataValue"/> class.
    /// </summary>
    /// <param name="property">The name of the JSON property that holds the value.</param>
    /// <param name="type">The type of the value.</param>
    public ContentDataValue(string property, DataFieldType type)
    {
        ArgumentException.ThrowIfNullOrEmpty(property);

        Property = property;
        Type = type;
    }

    /// <summary>
    /// Gets the name of the JSON property that holds the value.
    /// </summary>
    public string Property { get; }

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public DataFieldType Type { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the property holds a list, whose items are read as one text value
    /// separated by commas.
    /// </summary>
    public bool IsList { get; set; }
}
