using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Schema;

/// <summary>
/// Provides basic JSON Schema validation.
/// This is a lightweight in-house implementation that covers common validation scenarios.
/// </summary>
public static class RecipeStepSchemaValidator
{
    /// <summary>
    /// Validates a JSON node against a schema.
    /// </summary>
    /// <param name="schema">The schema to validate against.</param>
    /// <param name="instance">The JSON instance to validate.</param>
    /// <returns>A validation result.</returns>
    public static RecipeStepSchemaValidationResult Validate(RecipeStepSchema schema, JsonNode instance)
    {
        ArgumentNullException.ThrowIfNull(schema);

        var errors = new List<string>();
        ValidateNode(schema.SchemaObject, instance, "", errors);

        return errors.Count == 0
            ? RecipeStepSchemaValidationResult.Valid()
            : RecipeStepSchemaValidationResult.Invalid(errors);
    }

    private static void ValidateNode(JsonObject schema, JsonNode instance, string path, List<string> errors)
    {
        // Check type constraint.
        if (schema.TryGetPropertyValue("type", out var typeNode) && typeNode is JsonValue typeValue)
        {
            var expectedType = typeValue.GetValue<string>();
            if (!ValidateType(instance, expectedType))
            {
                errors.Add($"At '{path}': Expected type '{expectedType}' but got '{GetJsonType(instance)}'.");
                return; // No point in continuing if type is wrong.
            }
        }

        // Check const constraint.
        if (schema.TryGetPropertyValue("const", out var constNode))
        {
            if (!JsonNodesEqual(instance, constNode))
            {
                errors.Add($"At '{path}': Value must be '{constNode}'.");
            }
        }

        // Check enum constraint.
        if (schema.TryGetPropertyValue("enum", out var enumNode) && enumNode is JsonArray enumArray)
        {
            var found = false;
            foreach (var enumValue in enumArray)
            {
                if (JsonNodesEqual(instance, enumValue))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                errors.Add($"At '{path}': Value must be one of the enum values.");
            }
        }

        // Validate object properties.
        if (instance is JsonObject obj)
        {
            // Check required properties.
            if (schema.TryGetPropertyValue("required", out var requiredNode) && requiredNode is JsonArray requiredArray)
            {
                foreach (var req in requiredArray)
                {
                    if (req is JsonValue reqValue)
                    {
                        var propName = reqValue.GetValue<string>();
                        if (!obj.ContainsKey(propName))
                        {
                            errors.Add($"At '{path}': Required property '{propName}' is missing.");
                        }
                    }
                }
            }

            // Validate properties against their schemas.
            if (schema.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject propsSchema)
            {
                foreach (var prop in obj)
                {
                    if (propsSchema.TryGetPropertyValue(prop.Key, out var propSchemaNode) && propSchemaNode is JsonObject propSchema)
                    {
                        ValidateNode(propSchema, prop.Value, CombinePath(path, prop.Key), errors);
                    }
                }
            }

            // Check additionalProperties.
            if (schema.TryGetPropertyValue("additionalProperties", out var additionalNode))
            {
                if (additionalNode is JsonValue additionalValue && !additionalValue.GetValue<bool>())
                {
                    // additionalProperties: false - check for unknown properties.
                    var knownProps = new HashSet<string>();
                    if (schema.TryGetPropertyValue("properties", out var knownPropsNode) && knownPropsNode is JsonObject knownPropsSchema)
                    {
                        foreach (var prop in knownPropsSchema)
                        {
                            knownProps.Add(prop.Key);
                        }
                    }

                    foreach (var prop in obj)
                    {
                        if (!knownProps.Contains(prop.Key))
                        {
                            errors.Add($"At '{path}': Additional property '{prop.Key}' is not allowed.");
                        }
                    }
                }
            }
        }

        // Validate array items.
        if (instance is JsonArray arr && schema.TryGetPropertyValue("items", out var itemsNode) && itemsNode is JsonObject itemsSchema)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                ValidateNode(itemsSchema, arr[i], CombinePath(path, i.ToString()), errors);
            }
        }

        // Validate array min/max items.
        if (instance is JsonArray array)
        {
            if (schema.TryGetPropertyValue("minItems", out var minItemsNode) && minItemsNode is JsonValue minItemsValue)
            {
                var minItems = minItemsValue.GetValue<int>();
                if (array.Count < minItems)
                {
                    errors.Add($"At '{path}': Array must have at least {minItems} items.");
                }
            }

            if (schema.TryGetPropertyValue("maxItems", out var maxItemsNode) && maxItemsNode is JsonValue maxItemsValue)
            {
                var maxItems = maxItemsValue.GetValue<int>();
                if (array.Count > maxItems)
                {
                    errors.Add($"At '{path}': Array must have at most {maxItems} items.");
                }
            }
        }

        // Validate string constraints.
        if (instance is JsonValue strValue && strValue.GetValueKind() == System.Text.Json.JsonValueKind.String)
        {
            var str = strValue.GetValue<string>();

            if (schema.TryGetPropertyValue("minLength", out var minLenNode) && minLenNode is JsonValue minLenValue)
            {
                var minLen = minLenValue.GetValue<int>();
                if (str.Length < minLen)
                {
                    errors.Add($"At '{path}': String must be at least {minLen} characters.");
                }
            }

            if (schema.TryGetPropertyValue("maxLength", out var maxLenNode) && maxLenNode is JsonValue maxLenValue)
            {
                var maxLen = maxLenValue.GetValue<int>();
                if (str.Length > maxLen)
                {
                    errors.Add($"At '{path}': String must be at most {maxLen} characters.");
                }
            }
        }

        // Validate allOf.
        if (schema.TryGetPropertyValue("allOf", out var allOfNode) && allOfNode is JsonArray allOfArray)
        {
            foreach (var subSchema in allOfArray)
            {
                if (subSchema is JsonObject subSchemaObj)
                {
                    ValidateNode(subSchemaObj, instance, path, errors);
                }
            }
        }

        // Validate anyOf.
        if (schema.TryGetPropertyValue("anyOf", out var anyOfNode) && anyOfNode is JsonArray anyOfArray)
        {
            var anyValid = false;
            foreach (var subSchema in anyOfArray)
            {
                if (subSchema is JsonObject subSchemaObj)
                {
                    var subErrors = new List<string>();
                    ValidateNode(subSchemaObj, instance, path, subErrors);
                    if (subErrors.Count == 0)
                    {
                        anyValid = true;
                        break;
                    }
                }
            }
            if (!anyValid && anyOfArray.Count > 0)
            {
                errors.Add($"At '{path}': Value must match at least one of the anyOf schemas.");
            }
        }

        // Validate oneOf.
        if (schema.TryGetPropertyValue("oneOf", out var oneOfNode) && oneOfNode is JsonArray oneOfArray)
        {
            var matchCount = 0;
            foreach (var subSchema in oneOfArray)
            {
                if (subSchema is JsonObject subSchemaObj)
                {
                    var subErrors = new List<string>();
                    ValidateNode(subSchemaObj, instance, path, subErrors);
                    if (subErrors.Count == 0)
                    {
                        matchCount++;
                    }
                }
            }
            if (matchCount != 1 && oneOfArray.Count > 0)
            {
                errors.Add($"At '{path}': Value must match exactly one of the oneOf schemas (matched {matchCount}).");
            }
        }
    }

    private static bool ValidateType(JsonNode instance, string expectedType)
    {
        return expectedType switch
        {
            "object" => instance is JsonObject,
            "array" => instance is JsonArray,
            "string" => instance is JsonValue v && v.GetValueKind() == System.Text.Json.JsonValueKind.String,
            "integer" => instance is JsonValue vi && (vi.GetValueKind() == System.Text.Json.JsonValueKind.Number && IsInteger(vi)),
            "number" => instance is JsonValue vn && vn.GetValueKind() == System.Text.Json.JsonValueKind.Number,
            "boolean" => instance is JsonValue vb && (vb.GetValueKind() == System.Text.Json.JsonValueKind.True || vb.GetValueKind() == System.Text.Json.JsonValueKind.False),
            "null" => instance is null || (instance is JsonValue vnull && vnull.GetValueKind() == System.Text.Json.JsonValueKind.Null),
            _ => true,
        };
    }

    private static bool IsInteger(JsonValue value)
    {
        try
        {
            var d = value.GetValue<double>();
            return Math.Abs(d % 1) < double.Epsilon;
        }
        catch
        {
            return false;
        }
    }

    private static string GetJsonType(JsonNode node)
    {
        return node switch
        {
            JsonObject => "object",
            JsonArray => "array",
            JsonValue v => v.GetValueKind() switch
            {
                System.Text.Json.JsonValueKind.String => "string",
                System.Text.Json.JsonValueKind.Number => "number",
                System.Text.Json.JsonValueKind.True or System.Text.Json.JsonValueKind.False => "boolean",
                System.Text.Json.JsonValueKind.Null => "null",
                _ => "unknown",
            },
            null => "null",
            _ => "unknown",
        };
    }

    private static bool JsonNodesEqual(JsonNode a, JsonNode b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.ToJsonString() == b.ToJsonString();
    }

    private static string CombinePath(string basePath, string segment)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return "/" + segment;
        }
        return basePath + "/" + segment;
    }
}

/// <summary>
/// Represents the result of schema validation.
/// </summary>
public sealed class RecipeStepSchemaValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; private init; }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; private init; } = [];

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    public static RecipeStepSchemaValidationResult Valid()
        => new() { IsValid = true };

    /// <summary>
    /// Creates an invalid result with errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public static RecipeStepSchemaValidationResult Invalid(IEnumerable<string> errors)
        => new() { IsValid = false, Errors = errors.ToList() };
}
