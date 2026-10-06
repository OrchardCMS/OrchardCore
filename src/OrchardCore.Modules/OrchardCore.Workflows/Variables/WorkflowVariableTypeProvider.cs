using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Looks up the registered <see cref="IWorkflowVariableType"/> services by name.
/// </summary>
public sealed class WorkflowVariableTypeProvider : IWorkflowVariableTypeProvider
{
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
        => _typesByName.Values.ToList();

    /// <inheritdoc />
    public IWorkflowVariableType Get(string name)
        => !string.IsNullOrEmpty(name) && _typesByName.TryGetValue(name, out var type) ? type : null;
}
