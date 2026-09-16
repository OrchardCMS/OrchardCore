using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Restricts embedded settings content to its declared type without exposing standalone content identity.
/// </summary>
public static class EmbeddedContentItemApi
{
    private static readonly HashSet<string> s_forbiddenProperties = new(StringComparer.Ordinal)
    {
        nameof(ContentItem.ContentItemId),
        nameof(ContentItem.ContentItemVersionId),
        nameof(ContentItem.Id),
        nameof(ContentItem.Latest),
        nameof(ContentItem.Published),
        nameof(ContentItem.ModifiedUtc),
        nameof(ContentItem.PublishedUtc),
        nameof(ContentItem.CreatedUtc),
        nameof(ContentItem.Owner),
        nameof(ContentItem.Author),
    };

    /// <summary>
    /// Builds the writable schema for an embedded content type.
    /// </summary>
    public static JsonObject BuildSchema(ContentTypeDefinition definition, ContentOptions contentOptions, JsonSerializerOptions serializerOptions)
    {
        var schema = ContentItemSchemaBuilder.BuildSchema(definition, contentOptions, serializerOptions);
        var properties = schema["properties"]?.AsObject() ?? [];

        foreach (var propertyName in s_forbiddenProperties)
        {
            properties.Remove(propertyName);
        }

        var allowed = GetAllowedProperties(definition);
        foreach (var propertyName in properties.Select(property => property.Key).ToArray())
        {
            if (!allowed.Contains(propertyName))
            {
                properties.Remove(propertyName);
            }
        }

        if (properties[nameof(ContentItem.ContentType)] is JsonObject contentTypeSchema)
        {
            contentTypeSchema["const"] = definition.Name;
        }

        schema["title"] = definition.DisplayName;
        var description = definition.GetSettings<ContentTypeSettings>().Description;
        if (!string.IsNullOrWhiteSpace(description))
        {
            schema["description"] = description;
        }
        else
        {
            schema.Remove("description");
        }

        schema["required"] = new JsonArray();
        schema["additionalProperties"] = false;

        return schema;
    }

    /// <summary>
    /// Serializes declared parts and display text without standalone content metadata.
    /// </summary>
    public static JsonObject CreateSafeEnvelope(ContentItem contentItem, ContentTypeDefinition definition, JsonSerializerOptions serializerOptions)
    {
        var serialized = JsonSerializer.SerializeToNode(contentItem, serializerOptions)?.AsObject() ?? [];
        var allowed = GetAllowedProperties(definition);

        foreach (var propertyName in serialized.Select(property => property.Key).ToArray())
        {
            if (!allowed.Contains(propertyName))
            {
                serialized.Remove(propertyName);
            }
        }

        serialized[nameof(ContentItem.ContentType)] = definition.Name;

        return serialized;
    }

    /// <summary>
    /// Validates a partial update against the canonical embedded type.
    /// </summary>
    public static Dictionary<string, string[]> ValidateInput(
        string name,
        ContentTypeDefinition definition,
        JsonObject input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (input is null)
        {
            errors[string.Empty] = ["A JSON object payload is required."];
            return errors;
        }

        if (input.TryGetPropertyValue(nameof(ContentItem.ContentType), out var contentTypeNode))
        {
            string contentType;
            try
            {
                contentType = contentTypeNode?.GetValue<string>();
            }
            catch (InvalidOperationException)
            {
                contentType = null;
            }

            if (!string.Equals(contentType, name, StringComparison.Ordinal))
            {
                errors[nameof(ContentItem.ContentType)] =
                    [$"ContentType must exactly match the route name '{name}'."];
            }
        }

        var allowed = GetAllowedProperties(definition);
        foreach (var property in input)
        {
            if (s_forbiddenProperties.Contains(property.Key))
            {
                errors[property.Key] = [$"The '{property.Key}' property cannot be updated."];
            }
            else if (!allowed.Contains(property.Key))
            {
                errors[property.Key] =
                    [$"The '{property.Key}' property is not declared by custom settings type '{name}'."];
            }
        }

        foreach (var error in ValidateEnvelope(definition, input))
        {
            errors.TryAdd(error.Key, error.Value);
        }

        return errors;
    }

    /// <summary>
    /// Groups content validation failures by member.
    /// </summary>
    public static Dictionary<string, string[]> CreateValidationErrors(ContentValidateResult result)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var error in result.Errors)
        {
            var memberNames = error.MemberNames?.Any() == true
                ? error.MemberNames
                : [string.Empty];

            foreach (var memberName in memberNames)
            {
                if (!errors.TryGetValue(memberName, out var messages))
                {
                    messages = [];
                    errors[memberName] = messages;
                }

                messages.Add(error.ErrorMessage);
            }
        }

        return errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray(),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// Validates the object structure of declared parts and fields.
    /// </summary>
    public static Dictionary<string, string[]> ValidateEnvelope(
        ContentTypeDefinition definition,
        JsonObject envelope)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var part in definition.Parts)
        {
            if (!envelope.TryGetPropertyValue(part.Name, out var partNode))
            {
                continue;
            }

            if (partNode is not JsonObject partObject)
            {
                errors[part.Name] = [$"The '{part.Name}' property must be a JSON object."];
                continue;
            }

            foreach (var field in part.PartDefinition.Fields)
            {
                if (partObject.TryGetPropertyValue(field.Name, out var fieldNode) &&
                    fieldNode is not JsonObject)
                {
                    errors[$"{part.Name}.{field.Name}"] =
                        [$"The '{part.Name}.{field.Name}' property must be a JSON object."];
                }
            }
        }

        return errors;
    }

    private static HashSet<string> GetAllowedProperties(ContentTypeDefinition definition)
        => new(
            definition.Parts.Select(part => part.Name)
                .Append(nameof(ContentItem.ContentType))
                .Append(nameof(ContentItem.DisplayText)),
            StringComparer.Ordinal);

}
