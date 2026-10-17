namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// An expression scope over a list of fields: a formula refers to a field by its technical name in square brackets,
/// such as <c>[Price] * [Quantity]</c>, and the compiled expression reads the value at the field's position in the
/// row.
/// </summary>
public sealed class FieldListExpressionScope : IExpressionScope
{
    private readonly Dictionary<string, (DataField Field, int Index)> _fields = new(StringComparer.Ordinal);

    public FieldListExpressionScope(IReadOnlyList<DataField> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        for (var index = 0; index < fields.Count; index++)
        {
            _fields.TryAdd(fields[index].Name, (fields[index], index));
        }
    }

    public bool TryResolve(string key, out ExpressionFieldBinding binding)
    {
        if (key is null || !_fields.TryGetValue(key, out var entry))
        {
            binding = null;

            return false;
        }

        binding = new ExpressionFieldBinding
        {
            Key = key,
            DataType = entry.Field.Type,
            Index = entry.Index,
        };

        return true;
    }
}
