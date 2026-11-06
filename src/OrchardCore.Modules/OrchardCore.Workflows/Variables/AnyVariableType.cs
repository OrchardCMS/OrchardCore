using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Any value, unchanged. JSON values become their CLR form.
/// </summary>
public sealed class AnyVariableType : IWorkflowVariableType
{
    public static string Name => "any";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<AnyVariableType>("Any");

    public static string Editor => "json";

    public bool TryCoerce(object value, out object result)
    {
        result = value is JsonNode node ? VariableValues.FromJson(node) : value;

        return true;
    }
}
