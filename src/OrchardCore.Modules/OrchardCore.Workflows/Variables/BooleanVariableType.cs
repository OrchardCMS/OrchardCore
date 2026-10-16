using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// True or false. Booleans and the texts <c>true</c> and <c>false</c> (ignoring case) convert to it.
/// </summary>
public sealed class BooleanVariableType : IWorkflowVariableType
{
    public static string Name => "boolean";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<BooleanVariableType>("Yes or no");

    public static string Editor => "boolean";

    public bool TryCoerce(object value, out object result)
    {
        if (value is JsonNode node)
        {
            value = VariableValues.FromJson(node);
        }

        switch (value)
        {
            case null:
                result = null;
                return true;
            case bool flag:
                result = flag;
                return true;
            case string text when bool.TryParse(text.Trim(), out var parsed):
                result = parsed;
                return true;
            default:
                result = null;
                return false;
        }
    }
}
