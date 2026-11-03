using OrchardCore.Localization;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Describes a registered variable type with the static members of its class, and holds the one instance of the
/// type that converts every value. <c>services.AddWorkflowVariableType&lt;TType&gt;()</c> registers one per type.
/// </summary>
public sealed class WorkflowVariableTypeDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowVariableTypeDescriptor"/> class.
    /// </summary>
    /// <param name="name">The unique name of the type, for example <c>string</c>.</param>
    /// <param name="displayName">The name of the type shown in the designer.</param>
    /// <param name="editor">How the designer edits a default value of this type.</param>
    /// <param name="instance">The instance of the type, which converts the values.</param>
    public WorkflowVariableTypeDescriptor(string name, LocalizationSource displayName, string editor, IWorkflowVariableType instance)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(instance);

        Name = name;
        DisplayName = displayName;
        Editor = editor;
        Instance = instance;
    }

    /// <summary>
    /// Gets the unique name of the type, which <see cref="Models.WorkflowVariableDefinition.TypeName"/> refers to.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the name of the type shown in the designer, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets how the designer edits a default value of this type: <c>text</c>, <c>number</c>, <c>boolean</c>,
    /// <c>datetime</c>, <c>json</c>, or <c>none</c> when the type has no default value.
    /// </summary>
    public string Editor { get; }

    /// <summary>
    /// Gets the instance of the type, shared by every tenant scope, which converts the values.
    /// </summary>
    public IWorkflowVariableType Instance { get; }

    /// <summary>
    /// Describes a variable type from the static members of its class, with a new instance of it.
    /// </summary>
    /// <typeparam name="TType">The class of the type.</typeparam>
    /// <returns>The descriptor.</returns>
    public static WorkflowVariableTypeDescriptor Create<TType>()
        where TType : class, IWorkflowVariableType, new()
    {
        return new WorkflowVariableTypeDescriptor(TType.Name, TType.DisplayName, TType.Editor, new TType());
    }
}
