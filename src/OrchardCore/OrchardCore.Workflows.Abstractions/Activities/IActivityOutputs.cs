using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// An activity that produces values other activities can use. Each output can be bound to a workflow variable
/// in the designer; the activity sets its outputs with
/// <see cref="Models.WorkflowExecutionContext.SetActivityOutput"/>, and the engine writes the bound variables once
/// the activity has run.
/// </summary>
public interface IActivityOutputs
{
    /// <summary>
    /// Returns the outputs of the activity.
    /// </summary>
    IEnumerable<ActivityOutputDescriptor> GetOutputs();
}

/// <summary>
/// An output of an activity.
/// </summary>
public sealed class ActivityOutputDescriptor
{
    /// <summary>
    /// The name the activity sets the output with.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The <see cref="Services.IWorkflowVariableType.Name"/> of the values it produces, or <c>any</c>.
    /// </summary>
    public string TypeName { get; init; } = "any";

    /// <summary>
    /// The name shown in the designer.
    /// </summary>
    public LocalizedString DisplayName { get; init; }

    /// <summary>
    /// What the output is, shown under its name in the designer, or <see langword="null"/>.
    /// </summary>
    public LocalizedString Description { get; init; }
}
