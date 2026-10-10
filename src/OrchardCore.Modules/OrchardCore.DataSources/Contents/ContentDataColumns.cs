using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.DataSources.Contents;

/// <summary>
/// The columns of a content type: the common columns of every content item, followed by the values of its parts and
/// fields that <see cref="ContentDataSourceOptions"/> describes. The content items data source reads them, and steps
/// that write content items write them.
/// </summary>
public static class ContentDataColumns
{
    /// <summary>
    /// Builds the columns of a content type.
    /// </summary>
    /// <param name="definition">The content type.</param>
    /// <param name="options">The values of the parts and fields.</param>
    /// <param name="S">Localizes the labels.</param>
    /// <returns>The columns.</returns>
    public static List<ContentDataColumn> Build(ContentTypeDefinition definition, ContentDataSourceOptions options, IStringLocalizer S)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(S);

        var common = S["Content item"].Value;
        var columns = new List<ContentDataColumn>
        {
            new(new DataField("ContentItemId", S["Content item id"], DataFieldType.Text, common) { IsIdentifier = true, IsKeyFilterable = true }, item => item.ContentItemId),
            new(new DataField("ContentItemVersionId", S["Version id"], DataFieldType.Text, common) { IsIdentifier = true }, item => item.ContentItemVersionId),
            new(new DataField("ContentType", S["Content type"], DataFieldType.Text, common), item => item.ContentType),
            new(new DataField("DisplayText", S["Display text"], DataFieldType.Text, common), item => item.DisplayText) { Write = (item, value) => item.DisplayText = DataValues.ToText(value) },
            new(new DataField("Owner", S["Owner"], DataFieldType.Text, common)
            {
                IsIdentifier = true,
                IsKeyFilterable = true,
                References = [new("Users", "Users", "UserId")],
            }, item => item.Owner),
            new(new DataField("Author", S["Author"], DataFieldType.Text, common), item => item.Author),
            new(new DataField("Published", S["Published"], DataFieldType.Boolean, common), item => item.Published),
            new(new DataField("Latest", S["Latest"], DataFieldType.Boolean, common), item => item.Latest),
            new(new DataField("CreatedUtc", S["Created"], DataFieldType.DateTime, common), item => item.CreatedUtc),
            new(new DataField("ModifiedUtc", S["Modified"], DataFieldType.DateTime, common), item => item.ModifiedUtc),
            new(new DataField("PublishedUtc", S["Published on"], DataFieldType.DateTime, common), item => item.PublishedUtc),
        };

        foreach (var typePart in definition.Parts)
        {
            var partName = typePart.Name;
            var partLabel = typePart.DisplayName();

            if (options.Parts.TryGetValue(typePart.PartDefinition.Name, out var partValues))
            {
                foreach (var value in partValues)
                {
                    columns.Add(new(
                        new DataField($"{partName}.{value.Property}", $"{partLabel} {value.Property}", value.Type, partLabel),
                        item => ReadValue(((JsonObject)item.Content)[partName]?[value.Property], value))
                    {
                        Write = (item, data) => WriteValue(item, partName, null, value, data),
                    });
                }
            }

            foreach (var partField in typePart.PartDefinition.Fields)
            {
                if (!options.Fields.TryGetValue(partField.FieldDefinition.Name, out var fieldValues))
                {
                    continue;
                }

                var fieldName = partField.Name;
                var fieldLabel = partField.DisplayName();

                foreach (var value in fieldValues)
                {
                    var name = fieldValues.Length == 1 ? $"{partName}.{fieldName}" : $"{partName}.{fieldName}.{value.Property}";
                    var label = fieldValues.Length == 1 ? fieldLabel : $"{fieldLabel} {value.Property}";

                    columns.Add(new(
                        new DataField(name, label, value.Type, partLabel),
                        item => ReadValue(((JsonObject)item.Content)[partName]?[fieldName]?[value.Property], value))
                    {
                        Write = (item, data) => WriteValue(item, partName, fieldName, value, data),
                    });
                }
            }
        }

        return columns;
    }

    /// <summary>
    /// Reads the value of a part or field from its JSON node. A list becomes text separated by commas.
    /// </summary>
    /// <param name="node">The JSON node.</param>
    /// <param name="value">The description of the value.</param>
    /// <returns>The value, of the CLR type of its data type.</returns>
    public static object ReadValue(JsonNode node, ContentDataValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (node is null)
        {
            return null;
        }

        if (value.IsList || node is JsonArray)
        {
            if (node is not JsonArray array)
            {
                return DataValues.Coerce(node.ToString(), value.Type);
            }

            var items = array
                .Where(item => item is not null)
                .Select(item => item is JsonValue jsonValue ? DataValues.ToText(jsonValue) : item.ToJsonString())
                .Where(text => !string.IsNullOrEmpty(text))
                .ToArray();

            return items.Length == 0 ? null : string.Join(',', items);
        }

        return node is JsonValue scalar
            ? DataValues.Coerce(scalar, value.Type)
            : DataValues.Coerce(node.ToJsonString(), value.Type);
    }

    /// <summary>
    /// Writes the value of a part or a field into a content item. Text holding a list, separated by commas, becomes a
    /// JSON array.
    /// </summary>
    /// <param name="item">The content item.</param>
    /// <param name="partName">The name of the part.</param>
    /// <param name="fieldName">The name of the field, or <see langword="null"/> for a value of the part itself.</param>
    /// <param name="value">The description of the value.</param>
    /// <param name="data">The value.</param>
    public static void WriteValue(ContentItem item, string partName, string fieldName, ContentDataValue value, object data)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(value);

        var root = (JsonObject)item.Content;

        if (root[partName] is not JsonObject part)
        {
            part = [];
            root[partName] = part;
        }

        var target = part;

        if (fieldName is not null)
        {
            if (part[fieldName] is not JsonObject field)
            {
                field = [];
                part[fieldName] = field;
            }

            target = field;
        }

        target[value.Property] = ToJson(data, value);
    }

    private static JsonNode ToJson(object data, ContentDataValue value)
    {
        if (value.IsList)
        {
            var items = (DataValues.ToText(data) ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => (JsonNode)JsonValue.Create(item))
                .ToArray();

            return new JsonArray(items);
        }

        return DataValues.Coerce(data, value.Type) switch
        {
            null => null,
            string text => JsonValue.Create(text),
            long number => JsonValue.Create(number),
            decimal number => JsonValue.Create(number),
            bool flag => JsonValue.Create(flag),
            DateTime date => JsonValue.Create(value.Type == DataFieldType.Date ? date.Date : DateTime.SpecifyKind(date, DateTimeKind.Utc)),
            var other => JsonValue.Create(DataValues.ToText(other)),
        };
    }
}

/// <summary>
/// A column of a content type.
/// </summary>
public sealed class ContentDataColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDataColumn"/> class.
    /// </summary>
    /// <param name="field">The field of the column.</param>
    /// <param name="read">Reads the value of the column from a content item.</param>
    public ContentDataColumn(DataField field, Func<ContentItem, object> read)
    {
        Field = field;
        Read = read;
    }

    /// <summary>
    /// Gets the field of the column.
    /// </summary>
    public DataField Field { get; }

    /// <summary>
    /// Gets the delegate that reads the value of the column from a content item.
    /// </summary>
    public Func<ContentItem, object> Read { get; }

    /// <summary>
    /// Gets the delegate that writes a value of the column into a content item, or <see langword="null"/> when the
    /// column can't be written, such as the dates a content item gets when it is saved.
    /// </summary>
    public Action<ContentItem, object> Write { get; init; }
}
