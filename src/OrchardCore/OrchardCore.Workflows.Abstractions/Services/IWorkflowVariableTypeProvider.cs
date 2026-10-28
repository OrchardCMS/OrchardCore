namespace OrchardCore.Workflows.Services;

/// <summary>
/// The registered variable types (<c>services.AddWorkflowVariableType&lt;TType&gt;()</c>).
/// </summary>
public interface IWorkflowVariableTypeProvider
{
    /// <summary>
    /// Returns every registered type: the built-in types first, then the others by name, and <c>any</c> last.
    /// </summary>
    IReadOnlyList<WorkflowVariableTypeDescriptor> List();

    /// <summary>
    /// Returns the type with the given name (ignoring case), or <see langword="null"/>.
    /// </summary>
    /// <param name="name">The <see cref="IWorkflowVariableType.Name"/>.</param>
    WorkflowVariableTypeDescriptor GetDescriptor(string name);

    /// <summary>
    /// Returns the instance of the type with the given name (ignoring case), or <see langword="null"/>.
    /// </summary>
    /// <param name="name">The <see cref="IWorkflowVariableType.Name"/>.</param>
    IWorkflowVariableType Get(string name);
}
