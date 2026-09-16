using System.Text;
using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal static class OutputFormatter
{
    public static async Task WriteAsync(CommandOutput output, OutputFormat format, TextWriter writer, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (format == OutputFormat.None)
        {
            return;
        }

        var text = format switch
        {
            OutputFormat.Human => HumanOutputFormatter.Format(output,
                useColor: ReferenceEquals(writer, Console.Out) && !Console.IsOutputRedirected
                    && string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("NO_COLOR"))
                    && !string.Equals(System.Environment.GetEnvironmentVariable("TERM"), "dumb", StringComparison.OrdinalIgnoreCase)),
            OutputFormat.Json => FormatJson(output.Json),
            OutputFormat.Table => FormatTable(output.Json, output.TableColumns),
            OutputFormat.Csv => FormatCsv(output.Json, output.TableColumns),
            OutputFormat.Tsv => FormatTsv(output.Json, output.TableColumns),
            OutputFormat.Yaml => FormatYaml(output.Json),
            OutputFormat.Toml => FormatToml(output.Json),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

        await writer.WriteLineAsync(text.AsMemory(), cancellationToken);
    }

    internal static string FormatJson(JsonElement element)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            element.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal static string FormatTable(JsonElement element, IReadOnlyList<CliTableColumnMetadata>? tableColumns)
    {
        var rows = ExtractRows(element, tableColumns, out var headers);
        if ((tableColumns is null || tableColumns.Count == 0) && element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                rows = ExtractRows(items, null, out headers);
            }
            else
            {
                headers = ["Property", "Value"];
                rows = element.EnumerateObject().Select(property => new List<string> { property.Name, property.Value.ToString() }).ToList();
            }
        }

        if (rows.Count == 0)
        {
            return "No results.";
        }

        headers = headers.Select(FormatTerminalCell).ToList();
        rows = rows.Select(row => row.Select(FormatTerminalCell).ToList()).ToList();
        var widths = headers.Select(header => header.Length).ToArray();
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                widths[columnIndex] = Math.Max(widths[columnIndex], rows[rowIndex][columnIndex].Length);
            }
        }

        var builder = new StringBuilder();
        AppendRow(builder, headers, widths);
        AppendSeparator(builder, widths);
        foreach (var row in rows)
        {
            AppendRow(builder, row, widths);
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatCsv(JsonElement element, IReadOnlyList<CliTableColumnMetadata>? tableColumns)
    {
        var rows = ExtractRows(element, tableColumns, out var headers);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', headers.Select(QuoteCsv)));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(QuoteCsv)));
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatTsv(JsonElement element, IReadOnlyList<CliTableColumnMetadata>? tableColumns)
    {
        var rows = ExtractRows(element, tableColumns, out var headers);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join('\t', headers.Select(EscapeTsv)));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join('\t', row.Select(EscapeTsv)));
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatYaml(JsonElement element)
    {
        var builder = new StringBuilder();
        WriteYaml(builder, element, 0);
        return builder.ToString().TrimEnd();
    }

    private static string FormatToml(JsonElement element)
    {
        var builder = new StringBuilder();

        if (element.ValueKind == JsonValueKind.Object)
        {
            WriteTomlObject(builder, element, []);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            builder.Append("items = ").Append(FormatTomlValue(element)).AppendLine();
        }
        else if (element.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
        {
            builder.Append("value = ").Append(FormatTomlValue(element)).AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static List<List<string>> ExtractRows(JsonElement element, IReadOnlyList<CliTableColumnMetadata>? tableColumns, out List<string> headers)
    {
        headers = [];
        var rows = new List<List<string>>();

        if (tableColumns is { Count: > 0 })
        {
            headers = [.. tableColumns.Select(column => column.Heading)];
            if (TryGetRowSource(element, tableColumns, out var rowSource, out var rowSourceProperty))
            {
                foreach (var item in rowSource.EnumerateArray())
                {
                    rows.Add([.. tableColumns.Select(column => GetPropertyPathValue(
                        item,
                        RemoveRowSourcePrefix(column.PropertyPath, rowSourceProperty)))]);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    rows.Add([.. tableColumns.Select(column => GetPropertyPathValue(item, column.PropertyPath))]);
                }
            }
            else
            {
                rows.Add([.. tableColumns.Select(column => GetPropertyPathValue(element, column.PropertyPath))]);
            }

            return rows;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            var items = element.EnumerateArray().ToList();
            if (items.Count == 0)
            {
                return rows;
            }

            if (items[0].ValueKind == JsonValueKind.Object)
            {
                headers = [.. items[0].EnumerateObject().Select(property => property.Name)];
                foreach (var item in items)
                {
                    rows.Add([.. headers.Select(header => GetPropertyPathValue(item, header))]);
                }
            }
            else
            {
                headers.Add("Value");
                foreach (var item in items)
                {
                    rows.Add([item.ToString()]);
                }
            }

            return rows;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            headers = [.. element.EnumerateObject().Select(property => property.Name)];
            rows.Add([.. headers.Select(header => GetPropertyPathValue(element, header))]);
            return rows;
        }

        headers.Add("Value");
        rows.Add([element.ToString()]);
        return rows;
    }

    private static bool TryGetRowSource(
        JsonElement element,
        IReadOnlyList<CliTableColumnMetadata> tableColumns,
        out JsonElement rowSource,
        out string rowSourceProperty)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var column in tableColumns)
            {
                var firstSegment = column.PropertyPath
                    .Split('.', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault()?
                    .TrimEnd('[', ']');

                if (!string.IsNullOrEmpty(firstSegment) &&
                    TryGetPropertyIgnoreCase(element, firstSegment, out rowSource) &&
                    rowSource.ValueKind == JsonValueKind.Array)
                {
                    rowSourceProperty = firstSegment;
                    return true;
                }
            }
        }

        rowSource = default;
        rowSourceProperty = string.Empty;
        return false;
    }

    private static string RemoveRowSourcePrefix(string propertyPath, string rowSourceProperty)
    {
        var segments = propertyPath.Split('.', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length > 0 &&
            string.Equals(segments[0].TrimEnd('[', ']'), rowSourceProperty, StringComparison.OrdinalIgnoreCase))
        {
            return segments.Length == 2 ? segments[1] : string.Empty;
        }

        return propertyPath;
    }

    private static string GetPropertyPathValue(JsonElement element, string propertyPath)
    {
        var current = element;
        foreach (var segment in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !TryGetPropertyIgnoreCase(current, segment, out current))
            {
                return string.Empty;
            }
        }

        return current.ValueKind switch
        {
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Array => string.Join(", ", current.EnumerateArray().Select(item => item.ToString())),
            _ => current.ToString(),
        };
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.TryGetProperty(propertyName, out property))
        {
            return true;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private static void AppendRow(StringBuilder builder, List<string> values, int[] widths)
    {
        for (var index = 0; index < values.Count; index++)
        {
            builder.Append(values[index].PadRight(widths[index]));
            if (index < values.Count - 1)
            {
                builder.Append(" | ");
            }
        }

        builder.AppendLine();
    }

    private static void AppendSeparator(StringBuilder builder, int[] widths)
    {
        for (var index = 0; index < widths.Length; index++)
        {
            builder.Append('-', widths[index]);
            if (index < widths.Length - 1)
            {
                builder.Append("-+-");
            }
        }

        builder.AppendLine();
    }

    private static string FormatTerminalCell(string value)
    {
        var isUrl = Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                builder.Append($"\\u{(int)character:x4}");
            }
            else
            {
                builder.Append(character);
            }

            if (!isUrl && builder.Length > 80)
            {
                return builder.ToString(0, 77) + "...";
            }
        }

        return builder.ToString();
    }

    private static string EscapeTsv(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);

    private static string QuoteCsv(string value)
    {
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static void WriteTomlObject(StringBuilder builder, JsonElement element, IReadOnlyList<string> path)
    {
        var nestedObjects = new List<JsonProperty>();

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                nestedObjects.Add(property);
                continue;
            }

            builder
                .Append(QuoteToml(property.Name))
                .Append(" = ")
                .Append(FormatTomlValue(property.Value))
                .AppendLine();
        }

        foreach (var property in nestedObjects)
        {
            if (builder.Length > 0 && builder[^1] != '\n')
            {
                builder.AppendLine();
            }

            builder.AppendLine();
            var childPath = path.Append(property.Name).ToArray();
            builder.Append('[').Append(string.Join('.', childPath.Select(QuoteToml))).Append(']').AppendLine();
            WriteTomlObject(builder, property.Value, childPath);
        }
    }

    private static string FormatTomlValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => QuoteToml(element.GetString() ?? string.Empty),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Number => element.GetRawText(),
        JsonValueKind.Array => $"[{string.Join(", ", element.EnumerateArray()
            .Where(item => item.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            .Select(FormatTomlValue))}]",
        JsonValueKind.Object => $"{{ {string.Join(", ", element.EnumerateObject()
            .Where(property => property.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            .Select(property => $"{QuoteToml(property.Name)} = {FormatTomlValue(property.Value)}"))} }}",
        _ => throw new ArgumentOutOfRangeException(nameof(element)),
    };

    private static string QuoteToml(string value) => $"\"{JsonEncodedText.Encode(value)}\"";

    private static void WriteYaml(StringBuilder builder, JsonElement element, int indent)
    {
        var prefix = new string(' ', indent);
        if ((element.ValueKind == JsonValueKind.Object && !element.EnumerateObject().Any()) ||
            (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() == 0))
        {
            builder.Append(prefix).AppendLine(element.GetRawText());
            return;
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        builder.Append(prefix).Append(QuoteYaml(property.Name)).Append(':').AppendLine();
                        WriteYaml(builder, property.Value, indent + 2);
                    }
                    else
                    {
                        builder.Append(prefix).Append(QuoteYaml(property.Name)).Append(": ").Append(FormatScalar(property.Value)).AppendLine();
                    }
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        builder.Append(prefix).Append('-').AppendLine();
                        WriteYaml(builder, item, indent + 2);
                    }
                    else
                    {
                        builder.Append(prefix).Append("- ").Append(FormatScalar(item)).AppendLine();
                    }
                }

                break;
            default:
                builder.Append(prefix).Append(FormatScalar(element)).AppendLine();
                break;
        }
    }

    private static string FormatScalar(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => QuoteYaml(element.GetString() ?? string.Empty),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => "null",
        _ => element.ToString(),
    };

    private static string QuoteYaml(string value) => $"\"{JsonEncodedText.Encode(value)}\"";
}
