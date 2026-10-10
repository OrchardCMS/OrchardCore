using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Converts rows to and from JSON objects, for the JSON based file formats.
/// </summary>
internal static class JsonRecords
{
    public static void WriteObject(Utf8JsonWriter writer, IReadOnlyList<DataField> fields, object[] row)
    {
        writer.WriteStartObject();

        for (var index = 0; index < fields.Count; index++)
        {
            var field = fields[index];
            writer.WritePropertyName(field.Name);
            WriteValue(writer, index < row.Length ? row[index] : null, field.Type);
        }

        writer.WriteEndObject();
    }

    public static async IAsyncEnumerable<DataBatch> ToBatchesAsync(
        IAsyncEnumerable<JsonObject> objects,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        batchSize = Math.Max(1, batchSize);

        IReadOnlyList<DataField> fields = null;
        var pending = new List<JsonObject>(batchSize);
        var yielded = false;

        await foreach (var item in objects.WithCancellation(cancellationToken))
        {
            if (item is null)
            {
                continue;
            }

            pending.Add(item);

            if (pending.Count == batchSize)
            {
                fields ??= InferFields(pending);

                yield return new DataBatch(fields, pending.Select(record => ToRow(fields, record)).ToList());
                yielded = true;
                pending.Clear();
            }
        }

        if (pending.Count > 0 || !yielded)
        {
            fields ??= InferFields(pending);

            yield return new DataBatch(fields, pending.Select(record => ToRow(fields, record)).ToList());
        }
    }

    private static DataField[] InferFields(List<JsonObject> records)
    {
        var names = new List<string>();
        var types = new Dictionary<string, List<DataFieldType>>(StringComparer.Ordinal);

        foreach (var record in records)
        {
            foreach (var (name, node) in record)
            {
                if (!types.TryGetValue(name, out var list))
                {
                    list = [];
                    types[name] = list;
                    names.Add(name);
                }

                if (GetType(node) is { } type)
                {
                    list.Add(type);
                }
            }
        }

        return names
            .Select(name => new DataField(name, name, DataFileValues.InferType(types[name])))
            .ToArray();
    }

    private static object[] ToRow(IReadOnlyList<DataField> fields, JsonObject record)
    {
        var row = new object[fields.Count];

        for (var index = 0; index < fields.Count; index++)
        {
            if (!record.TryGetPropertyValue(fields[index].Name, out var node) || node is null)
            {
                continue;
            }

            row[index] = node is JsonValue value
                ? DataValues.Coerce(value, fields[index].Type)
                : DataValues.Coerce(node.ToJsonString(), fields[index].Type);
        }

        return row;
    }

    private static DataFieldType? GetType(JsonNode node)
    {
        if (node is null)
        {
            return null;
        }

        return node.GetValueKind() switch
        {
            JsonValueKind.Number => IsWholeNumber(node) ? DataFieldType.Integer : DataFieldType.Decimal,
            JsonValueKind.True or JsonValueKind.False => DataFieldType.Boolean,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => DataFieldType.Text,
        };
    }

    private static bool IsWholeNumber(JsonNode node)
    {
        var text = node.ToJsonString();

        return text.IndexOfAny(['.', 'e', 'E']) < 0 && long.TryParse(text, out _);
    }

    private static void WriteValue(Utf8JsonWriter writer, object value, DataFieldType type)
    {
        if (value is null)
        {
            writer.WriteNullValue();

            return;
        }

        switch (type)
        {
            case DataFieldType.Integer when DataValues.Coerce(value, DataFieldType.Integer) is long number:
                writer.WriteNumberValue(number);
                break;

            case DataFieldType.Decimal when DataValues.Coerce(value, DataFieldType.Decimal) is decimal number:
                writer.WriteNumberValue(number);
                break;

            case DataFieldType.Boolean when DataValues.Coerce(value, DataFieldType.Boolean) is bool flag:
                writer.WriteBooleanValue(flag);
                break;

            default:
                writer.WriteStringValue(DataFileValues.ToText(value, type));
                break;
        }
    }
}
