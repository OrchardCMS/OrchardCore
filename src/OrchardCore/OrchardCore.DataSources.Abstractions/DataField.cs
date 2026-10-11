namespace OrchardCore.DataSources;

/// <summary>
/// Describes one field of a data set: its stable technical name, the label shown to users, and its type.
/// </summary>
public sealed class DataField
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataField"/> class.
    /// </summary>
    public DataField()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataField"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the field.</param>
    /// <param name="displayName">The label shown to users.</param>
    /// <param name="type">The type of the field values.</param>
    /// <param name="group">The optional group the field is listed under.</param>
    public DataField(string name, string displayName, DataFieldType type, string group = null)
    {
        Name = name;
        DisplayName = displayName;
        Type = type;
        Group = group;
    }

    /// <summary>
    /// Gets or sets the stable technical name of the field. Saved definitions store this name, so it must not change
    /// between releases. It may contain dots, for example <c>Customer.Email</c>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the label shown to users.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the type of the field values.
    /// </summary>
    public DataFieldType Type { get; set; }

    /// <summary>
    /// Gets or sets the optional group the field is listed under, for example a content part name.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// Gets or sets an optional description shown as a hint.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field identifies a record, such as a primary or foreign key.
    /// </summary>
    public bool IsIdentifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data source applies an <see cref="DataFilterOperator.In"/>
    /// condition on this field exactly, so a consumer can read only the records whose value is in a list of keys.
    /// </summary>
    public bool IsKeyFilterable { get; set; }

    /// <summary>
    /// Gets or sets the data sets whose records this field's values identify.
    /// </summary>
    public IList<DataFieldReference> References { get; set; } = [];

    /// <summary>
    /// Creates a copy of this field with another name and label, keeping its type, group and description.
    /// </summary>
    /// <param name="name">The technical name of the copy.</param>
    /// <param name="displayName">The label of the copy, or <see langword="null"/> to use <paramref name="name"/>.</param>
    /// <returns>The copy.</returns>
    public DataField WithName(string name, string displayName = null)
        => new(name, displayName ?? name, Type, Group)
        {
            Description = Description,
            IsIdentifier = IsIdentifier,
        };
}
