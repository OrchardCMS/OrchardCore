using System.Text.Json.Nodes;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// A variable declared by a workflow type. At run time, the variable is the workflow property of the same name,
/// read and written with its type through <see cref="WorkflowExecutionContext.Variables"/>.
/// </summary>
public sealed class WorkflowVariableDefinition
{
    /// <summary>
    /// The name of the variable: a letter or an underscore, then letters, digits or underscores. Names are unique
    /// in a workflow type, ignoring case.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The <see cref="Services.IWorkflowVariableType.Name"/> of the variable's type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// The value the variable has when an instance starts, or <see langword="null"/> for none.
    /// </summary>
    public JsonNode DefaultValue { get; set; }

    /// <summary>
    /// What the variable is for, shown in the designer.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Whether the variable is an input: a workflow that runs this one as an activity sets it, and so does a caller
    /// that starts the workflow with an input of that name.
    /// </summary>
    public bool IsInput { get; set; }

    /// <summary>
    /// Whether the variable is an output: a workflow that runs this one as an activity gets its value back.
    /// </summary>
    public bool IsOutput { get; set; }

    /// <summary>
    /// Returns a deep copy of this definition.
    /// </summary>
    public WorkflowVariableDefinition Clone()
        => new()
        {
            Name = Name,
            TypeName = TypeName,
            DefaultValue = DefaultValue?.DeepClone(),
            Description = Description,
            IsInput = IsInput,
            IsOutput = IsOutput,
        };
}
