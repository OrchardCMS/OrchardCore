namespace OrchardCore.Workflows.Services;

/// <summary>
/// The registered <see cref="IWorkflowVariableType"/> services.
/// </summary>
public interface IWorkflowVariableTypeProvider
{
    /// <summary>
    /// Returns every registered type.
    /// </summary>
    IReadOnlyList<IWorkflowVariableType> List();

    /// <summary>
    /// Returns the type with the given name (ignoring case), or <see langword="null"/>.
    /// </summary>
    /// <param name="name">The <see cref="IWorkflowVariableType.Name"/>.</param>
    IWorkflowVariableType Get(string name);
}
