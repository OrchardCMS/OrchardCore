using System.Globalization;
using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A date and time, stored in UTC. Dates, and ISO 8601 texts (UTC when they have no offset), convert to it.
/// </summary>
public sealed class DateTimeVariableType : IWorkflowVariableType
{
    public static string Name => "datetime";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DateTimeVariableType>("Date and time");

    public static string Editor => "datetime";

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
            case DateTime dateTime:
                result = dateTime.Kind switch
                {
                    DateTimeKind.Local => dateTime.ToUniversalTime(),
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                    _ => dateTime,
                };
                return true;
            case DateTimeOffset dateTimeOffset:
                result = dateTimeOffset.UtcDateTime;
                return true;
            case string text when DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed):
                result = parsed.UtcDateTime;
                return true;
            default:
                result = null;
                return false;
        }
    }
}
