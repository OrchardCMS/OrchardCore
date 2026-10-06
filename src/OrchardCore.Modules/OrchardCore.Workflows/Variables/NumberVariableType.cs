using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A number, stored as a <see cref="double"/>. Numbers and text in the invariant culture convert to it.
/// </summary>
public sealed class NumberVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public NumberVariableType(IStringLocalizer<NumberVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "number";

    public LocalizedString DisplayName => S["Number"];

    public string Editor => "number";

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
