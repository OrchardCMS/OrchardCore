using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A JSON object, stored as a dictionary. Dictionaries, JSON objects (or their text) and other objects, through
/// their JSON form, convert to it.
/// </summary>
public sealed class ObjectVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public ObjectVariableType(IStringLocalizer<ObjectVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "object";

    public LocalizedString DisplayName => S["Object"];

    public string Editor => "json";

    public bool TryCoerce(object value, out object result)
    {
        result = null;

        switch (value)
        {
            case null:
                return true;
            case IDictionary<string, object> dictionary:
                result = dictionary;
                return true;
            case JsonObject jsonObject:
                result = VariableValues.FromJson(jsonObject);
                return true;
            case JsonNode:
                return false;
            case string text:
                return TryParse(text, out result);
            case bool or DateTime or DateTimeOffset:
                return false;
        }

        if (VariableValues.IsNumber(value) || VariableValues.IsList(value))
        {
            return false;
        }

        // Another object, for example an anonymous one: its JSON form.
        if (JsonSerializer.SerializeToNode(value, JOptions.Default) is JsonObject serialized)
        {
            result = VariableValues.FromJson(serialized);

            return true;
        }

        return false;
    }

    private static bool TryParse(string text, out object result)
    {
        result = null;

        try
        {
            if (JsonNode.Parse(text) is JsonObject jsonObject)
            {
                result = VariableValues.FromJson(jsonObject);

                return true;
            }
        }
        catch (JsonException)
        {
        }

        return false;
    }
}
