
namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Resolves the fields a formula may refer to.
/// </summary>
public interface IExpressionScope
{
    /// <summary>
    /// Resolves a field key.
    /// </summary>
    /// <param name="key">The field key as written in the formula.</param>
    /// <param name="binding">The resolved binding when found.</param>
    /// <returns><see langword="true"/> when the field exists.</returns>
    bool TryResolve(string key, out ExpressionFieldBinding binding);
}

/// <summary>
/// How a formula reads one field: from a slot of the current row, or by evaluating an aggregate calculated field over
/// the current group.
/// </summary>
public sealed class ExpressionFieldBinding
{
    /// <summary>
    /// Gets or sets the canonical key of the field.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the type of the field values.
    /// </summary>
    public DataFieldType DataType { get; set; }

    /// <summary>
    /// Gets or sets the index of the row slot that holds the field, for a row-level field.
    /// </summary>
    public int Index { get; set; } = -1;

    /// <summary>
    /// Gets or sets the compiled formula of an aggregate calculated field. When set, the field is aggregate.
    /// </summary>
    public CompiledExpression Aggregate { get; set; }

    /// <summary>
    /// Gets or sets the keys of the fields this field reads, when it is a calculated field.
    /// </summary>
    public IReadOnlySet<string> References { get; set; }
}
