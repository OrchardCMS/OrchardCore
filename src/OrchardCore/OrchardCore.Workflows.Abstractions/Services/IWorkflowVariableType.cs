using OrchardCore.Localization;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// A type of workflow variable: how values are converted to it. Register an implementation with
/// <c>services.AddWorkflowVariableType&lt;TType&gt;()</c>; its <see cref="Name"/> is what
/// <see cref="Models.WorkflowVariableDefinition.TypeName"/> refers to, and a later registration of a name replaces an
/// earlier one. The types are listed from their static members, without creating them.
/// </summary>
public interface IWorkflowVariableType
{
    /// <summary>
    /// Gets the unique name of the type, for example <c>string</c>.
    /// </summary>
    static abstract string Name { get; }

    /// <summary>
    /// Gets the name of the type shown in the designer.
    /// </summary>
    static abstract LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets how the designer edits a default value of this type: <c>text</c>, <c>number</c>, <c>boolean</c>,
    /// <c>datetime</c>, <c>json</c>, or <c>none</c> when the type has no default value.
    /// </summary>
    static abstract string Editor { get; }

    /// <summary>
    /// Converts a value to this type. A <see langword="null"/> value is always valid.
    /// </summary>
    /// <param name="value">The value, which can be a CLR value or a <see cref="System.Text.Json.Nodes.JsonNode"/>.</param>
    /// <param name="result">The converted value.</param>
    /// <returns>Whether the value could be converted.</returns>
    bool TryCoerce(object value, out object result);
}
