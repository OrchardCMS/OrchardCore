namespace OrchardCore.DataSources;

/// <summary>
/// A filter condition a consumer offers to a data source so the source can narrow what it reads. The values
/// are already converted to the field's CLR type. Applying a condition is optional: the engine applies every filter
/// again after reading, so a source may ignore any condition it cannot translate.
/// </summary>
public sealed class DataCondition
{
    /// <summary>
    /// Gets or sets the technical name of the data set field the condition applies to.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator.
    /// </summary>
    public DataFilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the comparison values, converted to the field's CLR type. An entry may be <see langword="null"/>
    /// for an open bound of <see cref="DataFilterOperator.Between"/>.
    /// </summary>
    public IList<object> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the condition carries the keys a join needs (an
    /// <see cref="DataFilterOperator.In"/> list of identifiers read from the data sets joined before), rather than a
    /// filter. It is sent only for fields marked <see cref="DataField.IsKeyFilterable"/>,
    /// whose source must then read only the records with one of these values. Identifiers keep their case, so the
    /// values may be compared exactly.
    /// </summary>
    public bool IsJoinKey { get; set; }
}
