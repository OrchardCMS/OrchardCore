using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

internal static class HumanErrorFormatter
{
    public static string Format(string message, JsonNode? details = null, string? correlationId = null)
    {
        var lines = new List<string> { "An error occurred.", Safe(message) };
        AppendDetails(lines, details);
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            lines.Add($"Request ID: {Safe(correlationId)}");
        }

        return string.Join(global::System.Environment.NewLine, lines.Distinct(StringComparer.Ordinal));
    }

    private static void AppendDetails(List<string> lines, JsonNode? details)
    {
        if (details is JsonObject problem)
        {
            // Problem Details, OAuth errors, and common JSON error envelopes.
            AddText(lines, problem["detail"] ?? problem["title"]);
            AddText(lines, problem["message"]);
            AddText(lines, problem["error_description"]);
            if (problem["error"] is { } error)
            {
                AppendDetails(lines, error);
            }

            if (problem["errors"] is JsonObject validation)
            {
                foreach (var field in validation)
                {
                    var messages = new List<string>();
                    AppendDetails(messages, field.Value);
                    lines.AddRange(messages.Select(value => string.IsNullOrEmpty(field.Key) ? value : $"{Safe(field.Key)}: {value}"));
                }
            }
            else if (problem["errors"] is JsonArray errors)
            {
                AppendDetails(lines, errors);
            }
        }
        else if (details is JsonArray array)
        {
            foreach (var item in array)
            {
                AppendDetails(lines, item);
            }
        }
        else
        {
            AddText(lines, details);
        }
    }

    private static void AddText(List<string> lines, JsonNode? value)
    {
        if (value is JsonValue scalar && scalar.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text))
        {
            lines.Add(Safe(text));
        }
    }

    // Error messages originate on the server. Keep text, not terminal controls.
    private static string Safe(string value) => new(value.Where(character => !char.IsControl(character) || character is '\n' or '\t').ToArray());
}
