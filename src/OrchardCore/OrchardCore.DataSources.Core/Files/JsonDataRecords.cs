using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Converts rows to and from JSON objects, for the JSON based file formats and for data sources that read JSON
/// documents.
/// </summary>
public static class JsonDataRecords
{
    /// <summary>
    /// Writes a row as a JSON object whose properties are the fields of the row, with typed values.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="fields">The fields of the row.</param>
    /// <param name="row">The row.</param>
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

    /// <summary>
    /// Converts JSON objects to batches of rows. The fields and their types are inferred from the objects of the first
    /// batch.
    /// </summary>
    /// <param name="objects">The objects.</param>
    /// <param name="batchSize">The number of rows per batch.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The batches. No objects yield one empty batch.</returns>
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

    /// <summary>
    /// Infers the fields of JSON objects: one field per property, in the order properties first appear, typed from
    /// their values. Nested objects and arrays are text.
    /// </summary>
    /// <param name="records">The objects.</param>
    /// <returns>The fields.</returns>
    public static DataField[] InferFields(IEnumerable<JsonObject> records)
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

    /// <summary>
    /// Converts a JSON object to a row of the given fields. Nested objects and arrays become JSON text.
    /// </summary>
    /// <param name="fields">The fields of the row.</param>
    /// <param name="record">The object.</param>
    /// <returns>The row.</returns>
    public static object[] ToRow(IReadOnlyList<DataField> fields, JsonObject record)
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
