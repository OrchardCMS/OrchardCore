using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Options;

/// <summary>
/// Holds the registered variable types. <c>services.AddWorkflowVariableType&lt;TType&gt;()</c> adds them.
/// </summary>
public sealed class WorkflowVariableTypeOptions
{
    private readonly List<WorkflowVariableTypeDescriptor> _types = [];

    /// <summary>
    /// Gets the registered types, in the order they were registered.
    /// </summary>
    public IReadOnlyList<WorkflowVariableTypeDescriptor> Types => _types;

    /// <summary>
    /// Registers a variable type. It replaces a type registered earlier with the same name (ignoring case), so a
    /// module can replace a built-in type.
    /// </summary>
    /// <param name="descriptor">The variable type.</param>
    /// <returns>The options.</returns>
    public WorkflowVariableTypeOptions Add(WorkflowVariableTypeDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        _types.RemoveAll(type => string.Equals(type.Name, descriptor.Name, StringComparison.OrdinalIgnoreCase));
        _types.Add(descriptor);

        return this;
    }
}
