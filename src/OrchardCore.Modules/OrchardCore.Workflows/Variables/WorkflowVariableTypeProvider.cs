using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Options;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// The default <see cref="IWorkflowVariableTypeProvider"/>, built from the types of
/// <see cref="WorkflowVariableTypeOptions"/>. The types need no services, so the options hold the one instance of
/// each type and the provider is a singleton.
/// </summary>
public sealed class WorkflowVariableTypeProvider : IWorkflowVariableTypeProvider
{
    // The order the designer offers the built-in types in; other types come after them, by name, and 'any' last.
    private static readonly string[] s_order = ["string", "number", "boolean", "datetime", "object", "array"];

    private readonly IReadOnlyList<WorkflowVariableTypeDescriptor> _types;
    private readonly Dictionary<string, WorkflowVariableTypeDescriptor> _typesByName = new(StringComparer.OrdinalIgnoreCase);

    public WorkflowVariableTypeProvider(IOptions<WorkflowVariableTypeOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // The options keep the last registration of a name, so a module can replace a built-in type.
        foreach (var type in options.Value.Types)
        {
            _typesByName[type.Name] = type;
        }

        _types = _typesByName.Values
            .OrderBy(Rank)
            .ThenBy(type => type.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<WorkflowVariableTypeDescriptor> List() => _types;

    /// <inheritdoc />
    public WorkflowVariableTypeDescriptor GetDescriptor(string name)
    {
        return string.IsNullOrEmpty(name) ? null : _typesByName.GetValueOrDefault(name);
    }

    /// <inheritdoc />
    public IWorkflowVariableType Get(string name)
    {
        return GetDescriptor(name)?.Instance;
    }

    private static int Rank(WorkflowVariableTypeDescriptor type)
    {
        if (string.Equals(type.Name, "any", StringComparison.OrdinalIgnoreCase))
        {
            return int.MaxValue;
        }

        var index = Array.FindIndex(s_order, name => string.Equals(name, type.Name, StringComparison.OrdinalIgnoreCase));

        return index < 0 ? s_order.Length : index;
    }
}
