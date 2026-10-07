using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// An activity that provides values to the activities that run after it: an event that starts or resumes the
/// workflow with input values, or a task that writes to the workflow's input, output or properties. The designer
/// lists them in the data available to the activities on a path from this one. A module can also declare the values
/// of an activity when it registers it, see <see cref="Options.ActivityRegistration.Provides"/>.
/// </summary>
public interface IActivityProvidedValues
{
    /// <summary>
    /// Returns the values the activity provides, given its current properties.
    /// </summary>
    IEnumerable<ActivityProvidedValue> GetProvidedValues();
}

/// <summary>
/// A value an activity provides.
/// </summary>
public sealed class ActivityProvidedValue
{
    /// <summary>
    /// Where the value is: the workflow's input, output or properties.
    /// </summary>
    public WorkflowValueSource Source { get; init; }

    /// <summary>
    /// The key of the value in its source.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The <see cref="Services.IWorkflowVariableType.Name"/> of the value, or <c>any</c>.
    /// </summary>
    public string TypeName { get; init; } = "any";

    /// <summary>
    /// What the value is, shown in the designer.
    /// </summary>
    public LocalizedString Description { get; init; }

    /// <summary>
    /// The fields of the value that scripts and Liquid templates read, for example the <c>ContentType</c> of a
    /// content item.
    /// </summary>
    public IReadOnlyList<ActivityProvidedValueMember> Members { get; init; } = [];
}

/// <summary>
/// A field of a value an activity provides (<see cref="ActivityProvidedValue.Members"/>).
/// </summary>
public sealed class ActivityProvidedValueMember
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The <see cref="Services.IWorkflowVariableType.Name"/> of the field, or <c>any</c>.
    /// </summary>
    public string TypeName { get; init; } = "any";

    /// <summary>
    /// What the field is, shown in the designer.
    /// </summary>
    public LocalizedString Description { get; init; }
}

/// <summary>
/// Where a value an activity provides is in the workflow (<see cref="ActivityProvidedValue.Source"/>).
/// </summary>
public enum WorkflowValueSource
{
    /// <summary>
    /// The workflow's input (<c>input('name')</c>, <c>{{ Workflow.Input.name }}</c>), which the workflow starts or
    /// resumes with.
    /// </summary>
    Input,

    /// <summary>
    /// The workflow's output (<c>output('name')</c>, <c>{{ Workflow.Output.name }}</c>), returned to whoever started
    /// the workflow.
    /// </summary>
    Output,

    /// <summary>
    /// The workflow's properties (<c>property('name')</c>, <c>{{ Workflow.Properties.name }}</c>).
    /// </summary>
    Properties,
}
