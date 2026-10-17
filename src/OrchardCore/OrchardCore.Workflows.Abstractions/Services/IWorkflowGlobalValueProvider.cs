using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Lists what every expression of a workflow can read without an activity providing it: global Liquid values, such
/// as the site's settings, and the functions scripts can call. The designer lists them in the data available to the
/// activities.
/// </summary>
public interface IWorkflowGlobalValueProvider
{
    /// <summary>
    /// Returns the global values and functions this provider knows.
    /// </summary>
    IEnumerable<WorkflowGlobalValue> GetGlobalValues();
}

/// <summary>
/// A value or a function that every expression of a workflow can use.
/// </summary>
public sealed class WorkflowGlobalValue
{
    /// <summary>
    /// Whether it is a value or a function.
    /// </summary>
    public WorkflowGlobalValueKind Kind { get; init; }

    /// <summary>
    /// The name shown in the designer, for example <c>Site</c> or <c>uuid()</c>.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The <see cref="IWorkflowVariableType.Name"/> of the value, or of what the function returns, or <c>any</c>.
    /// </summary>
    public string TypeName { get; init; } = "any";

    /// <summary>
    /// What it is, shown in the designer.
    /// </summary>
    public LocalizedString Description { get; init; }

    /// <summary>
    /// The Liquid path that reads the value, for example <c>Site</c>, or <see langword="null"/> when Liquid can't
    /// read it.
    /// </summary>
    public string LiquidPath { get; init; }

    /// <summary>
    /// The JavaScript that reads the value or calls the function, for example <c>uuid()</c>, or
    /// <see langword="null"/> when scripts can't.
    /// </summary>
    public string JavaScript { get; init; }

    /// <summary>
    /// The fields of the value, for example the <c>SiteName</c> of <c>Site</c>. A name can be a path, such as
    /// <c>Identity.Name</c>.
    /// </summary>
    public IReadOnlyList<ActivityProvidedValueMember> Members { get; init; } = [];

    /// <summary>
    /// A value that Liquid templates read.
    /// </summary>
    public static WorkflowGlobalValue Liquid(string path, string typeName, LocalizedString description, IReadOnlyList<ActivityProvidedValueMember> members = null)
        => new()
        {
            Kind = WorkflowGlobalValueKind.Value,
            Name = path,
            TypeName = typeName,
            Description = description,
            LiquidPath = path,
            Members = members ?? [],
        };

    /// <summary>
    /// A function that scripts call, for example <c>uuid()</c>.
    /// </summary>
    public static WorkflowGlobalValue Function(string call, string typeName, LocalizedString description)
        => new()
        {
            Kind = WorkflowGlobalValueKind.Function,
            Name = call,
            TypeName = typeName,
            Description = description,
            JavaScript = call,
        };
}

/// <summary>
/// What a <see cref="WorkflowGlobalValue"/> is.
/// </summary>
public enum WorkflowGlobalValueKind
{
    /// <summary>
    /// A value expressions read, such as <c>Site</c>.
    /// </summary>
    Value,

    /// <summary>
    /// A function scripts call, such as <c>uuid()</c>.
    /// </summary>
    Function,
}
