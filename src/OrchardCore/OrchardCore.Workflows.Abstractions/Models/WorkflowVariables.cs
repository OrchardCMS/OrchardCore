using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// The variables of a running workflow. A variable is the workflow property of the same name
/// (<see cref="WorkflowExecutionContext.Properties"/>): declared variables are converted to their type when they
/// are read and written here, other names are read and written as they are.
/// </summary>
public sealed class WorkflowVariables
{
    private readonly IDictionary<string, object> _properties;
    private readonly Dictionary<string, WorkflowVariableDefinition> _definitions;
    private readonly Func<string, IWorkflowVariableType> _typeResolver;

    /// <summary>
    /// Creates the variables of a workflow.
    /// </summary>
    /// <param name="properties">The workflow properties, which hold the values.</param>
    /// <param name="definitions">The declared variables.</param>
    /// <param name="typeResolver">Returns the type of a name, or <see langword="null"/> when it isn't registered
    /// (the values of such variables are then kept as they are).</param>
    public WorkflowVariables(
        IDictionary<string, object> properties,
        IEnumerable<WorkflowVariableDefinition> definitions,
        Func<string, IWorkflowVariableType> typeResolver)
    {
        ArgumentNullException.ThrowIfNull(properties);

        _properties = properties;
        _definitions = new Dictionary<string, WorkflowVariableDefinition>(StringComparer.OrdinalIgnoreCase);
        _typeResolver = typeResolver ?? (_ => null);

        foreach (var definition in definitions ?? [])
        {
            if (!string.IsNullOrEmpty(definition?.Name))
            {
                _definitions.TryAdd(definition.Name, definition);
            }
        }
    }

    /// <summary>
    /// The declared variables.
    /// </summary>
    public IReadOnlyCollection<WorkflowVariableDefinition> Definitions => _definitions.Values;

    /// <summary>
    /// Gets or sets a variable; see <see cref="Get"/> and <see cref="Set"/>.
    /// </summary>
    public object this[string name]
    {
        get => Get(name);
        set => Set(name, value);
    }

    /// <summary>
    /// Whether <paramref name="name"/> is a declared variable (ignoring case).
    /// </summary>
    public bool IsDeclared(string name)
        => !string.IsNullOrEmpty(name) && _definitions.ContainsKey(name);

    /// <summary>
    /// Returns the definition of a declared variable, or <see langword="null"/>.
    /// </summary>
    public WorkflowVariableDefinition GetDefinition(string name)
        => !string.IsNullOrEmpty(name) && _definitions.TryGetValue(name, out var definition) ? definition : null;

    /// <summary>
    /// Returns the value of a variable, or <see langword="null"/> when it has none. A declared variable's value is
    /// converted to its type; a value that doesn't convert (written through the properties) is returned as it is.
    /// </summary>
    public object Get(string name)
        => TryGetValue(name, out var value) ? value : null;

    /// <summary>
    /// Returns whether a variable has a value, and the value; see <see cref="Get"/>.
    /// </summary>
    public bool TryGetValue(string name, out object value)
    {
        value = null;

        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        var definition = GetDefinition(name);
        var key = definition?.Name ?? name;

        if (!_properties.TryGetValue(key, out var stored))
        {
            return false;
        }

        value = definition is not null && _typeResolver(definition.TypeName) is { } type && type.TryCoerce(stored, out var coerced) ? coerced : stored;

        return true;
    }

    /// <summary>
    /// Sets a variable. A declared variable's value is converted to its type.
    /// </summary>
    /// <exception cref="WorkflowVariableException">The value doesn't convert to the variable's type.</exception>
    public void Set(string name, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var definition = GetDefinition(name);

        if (definition is null)
        {
            _properties[name] = value;

            return;
        }

        var type = _typeResolver(definition.TypeName);

        if (type is null)
        {
            _properties[definition.Name] = value;

            return;
        }

        if (!type.TryCoerce(value, out var coerced))
        {
            throw new WorkflowVariableException(definition.Name, $"The variable '{definition.Name}' can't hold this value: it isn't a valid {type.DisplayName}.");
        }

        _properties[definition.Name] = coerced;
    }

    /// <summary>
    /// Sets the declared variables that have no value yet to their default value.
    /// </summary>
    /// <summary>
    /// Sets the input variables (<see cref="WorkflowVariableDefinition.IsInput"/>) from the values of the same name
    /// in <paramref name="input"/>, converted to their types. Returns the names of the values that don't convert,
    /// which are left out.
    /// </summary>
    public IReadOnlyList<string> ApplyInputs(IDictionary<string, object> input)
    {
        if (input is null || input.Count == 0)
        {
            return [];
        }

        var failed = new List<string>();

        foreach (var definition in _definitions.Values.Where(definition => definition.IsInput))
        {
            var value = input.FirstOrDefault(entry => string.Equals(entry.Key, definition.Name, StringComparison.OrdinalIgnoreCase));

            if (value.Key is null)
            {
                continue;
            }

            try
            {
                Set(definition.Name, value.Value);
            }
            catch (WorkflowVariableException)
            {
                failed.Add(definition.Name);
            }
        }

        return failed;
    }

    /// <summary>
    /// Returns the values of the output variables (<see cref="WorkflowVariableDefinition.IsOutput"/>) that have one.
    /// </summary>
    public IDictionary<string, object> GetOutputs()
    {
        var outputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in _definitions.Values.Where(definition => definition.IsOutput))
        {
            if (TryGetValue(definition.Name, out var value))
            {
                outputs[definition.Name] = value;
            }
        }

        return outputs;
    }

    public void ApplyDefaults()
    {
        foreach (var definition in _definitions.Values)
        {
            if (definition.DefaultValue is null || _properties.ContainsKey(definition.Name))
            {
                continue;
            }

            var type = _typeResolver(definition.TypeName);

            if (type is not null && type.TryCoerce(definition.DefaultValue, out var value))
            {
                _properties[definition.Name] = value;
            }
        }
    }

    /// <summary>
    /// Returns the declared variables and their values.
    /// </summary>
    public IDictionary<string, object> ToDictionary()
        => _definitions.Values.ToDictionary(definition => definition.Name, definition => Get(definition.Name), StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Thrown when a value is written to a variable whose type it doesn't convert to.
/// </summary>
public sealed class WorkflowVariableException : InvalidOperationException
{
    public WorkflowVariableException(string variableName, string message)
        : base(message)
    {
        VariableName = variableName;
    }

    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string VariableName { get; }
}
