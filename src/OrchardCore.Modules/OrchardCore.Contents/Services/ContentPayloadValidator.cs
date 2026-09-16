using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Contents.Services;

// Validate with the same serializer and registered CLR types as ContentElement.Get.
// Do this outside content handlers, whose exception handling logs conversion failures.
internal sealed class ContentPayloadValidator
{
    private readonly IContentDefinitionManager _definitions;
    private readonly ContentOptions _options;

    public ContentPayloadValidator(IContentDefinitionManager definitions, ContentOptions options)
    {
        _definitions = definitions;
        _options = options;
    }

    internal static ContentItem ReadPayload(JsonObject payload, ModelStateDictionary errors)
    {
        if (payload is null)
        {
            errors.AddModelError(string.Empty, "A content item payload is required.");
            return null;
        }

        // ContentItemConverter is intentionally permissive for stored documents.
        // Check its well-known properties before it can discard invalid input.
        if (!TryDeserialize(payload, typeof(ContentItemSchemaBuilder.ContentItemSchema), string.Empty, errors, out _))
        {
            return null;
        }

        return payload.Deserialize<ContentItem>(JOptions.Default);
    }

    internal async Task ValidateAsync(JsonObject payload, string contentType, ModelStateDictionary errors, string prefix = "")
    {
        var definition = await _definitions.GetTypeDefinitionAsync(contentType);
        if (definition is null)
        {
            errors.AddModelError(prefix + nameof(ContentItem.ContentType), "The content type does not exist.");
            return;
        }

        foreach (var part in definition.Parts)
        {
            if (!payload.TryGetPropertyValue(part.Name, out var node) || node is null)
            {
                continue;
            }

            var path = prefix + part.Name;
            if (node is not JsonObject partData)
            {
                errors.AddModelError(path, "Expected a JSON object for the content part.");
                continue;
            }

            var type = _options.ContentPartOptionsLookup.TryGetValue(part.PartDefinition.Name, out var option)
                ? option.Type : typeof(ContentPart);
            await ValidateElementAsync(partData, type, path, errors);

            // Dynamic fields are not CLR properties of their containing part.
            foreach (var field in part.PartDefinition.Fields)
            {
                if (!partData.TryGetPropertyValue(field.Name, out var fieldNode) || fieldNode is null)
                {
                    continue;
                }

                var fieldPath = path + "." + field.Name;
                if (fieldNode is not JsonObject fieldData)
                {
                    errors.AddModelError(fieldPath, "Expected a JSON object for the content field.");
                    continue;
                }

                var fieldType = _options.ContentFieldOptionsLookup.TryGetValue(field.FieldDefinition.Name, out var fieldOption)
                    ? fieldOption.Type : typeof(ContentField);
                await ValidateElementAsync(fieldData, fieldType, fieldPath, errors);
            }
        }
    }

    private async Task ValidateElementAsync(JsonObject data, Type type, string path, ModelStateDictionary errors)
    {
        if (TryDeserialize(data, type, path, errors, out var value))
        {
            await ValidateEmbeddedItemsAsync(data, value, path, errors);
        }
    }

    private async Task ValidateEmbeddedItemsAsync(JsonNode data, object value, string path, ModelStateDictionary errors)
    {
        if (data is null || value is null)
        {
            return;
        }

        if (value is ContentItem && data is JsonObject itemData)
        {
            var itemErrors = new ModelStateDictionary();
            var item = ReadPayload(itemData, itemErrors);
            foreach (var error in itemErrors)
            {
                foreach (var message in error.Value.Errors)
                {
                    errors.AddModelError(path + "." + error.Key, message.ErrorMessage);
                }
            }

            if (item is not null)
            {
                if (string.IsNullOrWhiteSpace(item.ContentType))
                {
                    errors.AddModelError(path + ".ContentType", "ContentType is required for an embedded content item.");
                }
                else
                {
                    await ValidateAsync(itemData, item.ContentType, errors, path + ".");
                }
            }
        }
        else if (data is JsonArray array && value is IEnumerable sequence)
        {
            var index = 0;
            foreach (var entry in sequence)
            {
                if (index >= array.Count)
                {
                    break;
                }

                await ValidateEmbeddedItemsAsync(array[index], entry, $"{path}[{index}]", errors);
                index++;
            }
        }
        else if (data is JsonObject jsonObject)
        {
            if (value is IDictionary dictionary)
            {
                foreach (var property in jsonObject)
                {
                    if (dictionary.Contains(property.Key))
                    {
                        await ValidateEmbeddedItemsAsync(property.Value, dictionary[property.Key], path + "." + property.Key, errors);
                    }
                }
            }
            else
            {
                var properties = JOptions.Default.GetTypeInfo(value.GetType()).Properties;
                foreach (var child in jsonObject)
                {
                    var property = properties.FirstOrDefault(property => string.Equals(property.Name, child.Key,
                        JOptions.Default.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                    if (property?.Get is not null)
                    {
                        await ValidateEmbeddedItemsAsync(child.Value, property.Get(value), path + "." + child.Key, errors);
                    }
                }
            }
        }
    }

    private static bool TryDeserialize(JsonNode data, Type type, string prefix, ModelStateDictionary errors, out object value)
    {
        try
        {
            value = data.Deserialize(type, JOptions.Default);
            return true;
        }
        catch (JsonException exception)
        {
            var suffix = exception.Path?.TrimStart('$') ?? string.Empty;
            var path = (prefix + suffix).TrimStart('.');
            errors.AddModelError(path, "The value has an invalid JSON type or format for this property.");
            value = null;
            return false;
        }
    }
}
