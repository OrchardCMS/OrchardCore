using System.Globalization;
using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A number, stored as a <see cref="double"/>. Numbers and text in the invariant culture convert to it.
/// </summary>
public sealed class NumberVariableType : IWorkflowVariableType
{
    public static string Name => "number";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<NumberVariableType>("Number");

    public static string Editor => "number";

    public bool TryCoerce(object value, out object result)
    {
        if (value is JsonNode node)
        {
            value = VariableValues.FromJson(node);
        }

        if (value is null)
        {
            result = null;

            return true;
        }

        if (VariableValues.IsNumber(value))
        {
            result = VariableValues.ToDouble(value);

            return true;
        }

        if (value is string text && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            result = number;

            return true;
        }

        result = null;

        return false;
    }
}
