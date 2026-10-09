using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A list, stored as a <see cref="List{T}"/>. Lists and JSON arrays (or their text) convert to it.
/// </summary>
public sealed class ArrayVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public ArrayVariableType(IStringLocalizer<ArrayVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "array";

    public LocalizedString DisplayName => S["List"];

    public string Editor => "json";

    public bool TryCoerce(object value, out object result)
    {
        result = null;

        switch (value)
        {
            case null:
                return true;
            case JsonArray jsonArray:
                result = VariableValues.FromJson(jsonArray);
                return true;
            case JsonNode:
                return false;
            case string text:
                return TryParse(text, out result);
            case List<object> list:
                result = list;
                return true;
        }

        if (VariableValues.IsList(value))
        {
            result = ((IEnumerable)value).Cast<object>().ToList();

            return true;
        }

        return false;
    }

    private static bool TryParse(string text, out object result)
    {
        result = null;

        try
        {
            if (JsonNode.Parse(text) is JsonArray jsonArray)
            {
                result = VariableValues.FromJson(jsonArray);

                return true;
            }
        }
        catch (JsonException)
        {
        }

        return false;
    }
}
