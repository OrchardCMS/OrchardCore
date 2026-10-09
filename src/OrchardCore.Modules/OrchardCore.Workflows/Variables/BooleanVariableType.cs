using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// True or false. Booleans and the texts <c>true</c> and <c>false</c> (ignoring case) convert to it.
/// </summary>
public sealed class BooleanVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public BooleanVariableType(IStringLocalizer<BooleanVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "boolean";

    public LocalizedString DisplayName => S["Yes or no"];

    public string Editor => "boolean";

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
