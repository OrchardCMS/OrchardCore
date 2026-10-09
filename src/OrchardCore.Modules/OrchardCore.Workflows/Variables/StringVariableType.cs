using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Text. Every value converts to it: numbers and dates in the invariant culture, objects and lists as JSON.
/// </summary>
public sealed class StringVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public StringVariableType(IStringLocalizer<StringVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "string";

    public LocalizedString DisplayName => S["Text"];

    public string Editor => "text";

    public bool TryCoerce(object value, out object result)
    {
        result = value switch
        {
            null => null,
            string text => text,
            JsonValue jsonValue => VariableValues.FromJson(jsonValue) switch
            {
                string text => text,
                bool flag => flag ? "true" : "false",
                double number => number.ToString(CultureInfo.InvariantCulture),
                _ => jsonValue.ToJsonString(),
            },
            JsonNode node => node.ToJsonString(),
            bool flag => flag ? "true" : "false",
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ when VariableValues.IsList(value) || value is IDictionary<string, object> => JsonSerializer.Serialize(value, JOptions.Default),
            _ => value.ToString(),
        };

        return true;
    }
}
