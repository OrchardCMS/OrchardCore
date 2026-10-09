using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// A type of workflow variable: how values are converted to it. Register implementations as services; their
/// <see cref="Name"/> is what <see cref="Models.WorkflowVariableDefinition.TypeName"/> refers to.
/// </summary>
public interface IWorkflowVariableType
{
    /// <summary>
    /// The unique name of the type, for example <c>string</c>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The name of the type shown in the designer.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// How the designer edits a default value of this type: <c>text</c>, <c>number</c>, <c>boolean</c>,
    /// <c>datetime</c>, <c>json</c>, or <c>none</c> when the type has no default value.
    /// </summary>
    string Editor { get; }

    /// <summary>
    /// Converts a value to this type. A <see langword="null"/> value is always valid.
    /// </summary>
    /// <param name="value">The value, which can be a CLR value or a <see cref="System.Text.Json.Nodes.JsonNode"/>.</param>
    /// <param name="result">The converted value.</param>
    /// <returns>Whether the value could be converted.</returns>
    bool TryCoerce(object value, out object result);
}
