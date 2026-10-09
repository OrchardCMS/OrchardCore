using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Looks up the registered <see cref="IWorkflowVariableType"/> services by name.
/// </summary>
public sealed class WorkflowVariableTypeProvider : IWorkflowVariableTypeProvider
{
    // The order the designer offers the built-in types in; other types come after them, by name, and 'any' last.
    private static readonly string[] s_order = ["string", "number", "boolean", "datetime", "object", "array"];

    private readonly IReadOnlyList<IWorkflowVariableType> _types;
    private readonly Dictionary<string, IWorkflowVariableType> _typesByName;

    public WorkflowVariableTypeProvider(IEnumerable<IWorkflowVariableType> types)
    {
        _types = types.ToList();
        _typesByName = new Dictionary<string, IWorkflowVariableType>(StringComparer.OrdinalIgnoreCase);

        // The last registration of a name wins, so a module can replace a built-in type.
        foreach (var type in _types)
        {
            _typesByName[type.Name] = type;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IWorkflowVariableType> List()
        => _typesByName.Values
            .OrderBy(Rank)
            .ThenBy(type => type.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static int Rank(IWorkflowVariableType type)
    {
        if (string.Equals(type.Name, "any", StringComparison.OrdinalIgnoreCase))
        {
            return int.MaxValue;
        }

        var index = Array.FindIndex(s_order, name => string.Equals(name, type.Name, StringComparison.OrdinalIgnoreCase));

        return index < 0 ? s_order.Length : index;
    }

    /// <inheritdoc />
    public IWorkflowVariableType Get(string name)
        => !string.IsNullOrEmpty(name) && _typesByName.TryGetValue(name, out var type) ? type : null;
}
