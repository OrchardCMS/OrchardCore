using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal static class OpenApiCliParser
{
    public static IReadOnlyList<OpenApiOperationDefinition> Parse(string openApiJson)
    {
        using var document = JsonDocument.Parse(openApiJson);
        return Parse(document.RootElement);
    }

    public static IReadOnlyList<OpenApiOperationDefinition> Parse(JsonElement root)
    {
        var operations = new List<OpenApiOperationDefinition>();
        if (!root.TryGetProperty("paths", out var pathsElement) || pathsElement.ValueKind != JsonValueKind.Object)
        {
            return operations;
        }

        foreach (var pathProperty in pathsElement.EnumerateObject())
        {
            if (pathProperty.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var methodProperty in pathProperty.Value.EnumerateObject())
            {
                if (!IsHttpMethod(methodProperty.Name) || methodProperty.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!TryParseCliMetadata(methodProperty.Value, root, out var metadata))
                {
                    continue;
                }

                var operation = new OpenApiOperationDefinition
                {
                    Method = methodProperty.Name.ToUpperInvariant(),
                    Path = pathProperty.Name,
                    OperationId = CliUtilities.ReadString(methodProperty.Value, "operationId"),
                    Summary = CliUtilities.ReadString(methodProperty.Value, "summary"),
                    Description = CliUtilities.ReadString(methodProperty.Value, "description"),
                    CliMetadata = metadata,
                };

                ParseParameters(operation, pathProperty.Value, root);
                ParseParameters(operation, methodProperty.Value, root);
                ParseRequestBody(operation, methodProperty.Value, root);
                operations.Add(operation);
            }
        }

        return operations;
    }

    private static List<string> ReadSchemaTypes(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object || !schema.TryGetProperty("type", out var type))
        {
            return [];
        }

        return type.ValueKind == JsonValueKind.Array
            ? [.. type.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String).Select(value => value.GetString()!)]
            : type.ValueKind == JsonValueKind.String ? [type.GetString()!] : [];
    }

    private static bool TryParseCliMetadata(JsonElement operationElement, JsonElement root, out CliOperationMetadata metadata)
    {
        metadata = null!;

        if (!operationElement.TryGetProperty(RemoteManagementConstants.CliExtensionName, out var extensionElement))
        {
            return false;
        }

        extensionElement = ResolveReferences(extensionElement, root);
        var commandGroup = CliUtilities.ReadStringArray(extensionElement, "commandGroup");
        var verb = CliUtilities.ReadString(extensionElement, "verb");

        if (commandGroup.Count == 0 || string.IsNullOrWhiteSpace(verb))
        {
            return false;
        }

        metadata = new CliOperationMetadata(commandGroup, verb)
        {
            Capability = CliUtilities.ReadString(extensionElement, "capability"),
            RequiresConfirmation = extensionElement.TryGetProperty("requiresConfirmation", out var requiresConfirmationElement) && requiresConfirmationElement.ValueKind == JsonValueKind.True,
            FileResponse = extensionElement.TryGetProperty("fileResponse", out var fileResponse) && fileResponse.ValueKind == JsonValueKind.True,
            SecretResponse = extensionElement.TryGetProperty("secretResponse", out var secretResponse) && secretResponse.ValueKind == JsonValueKind.True,
            Hidden = extensionElement.TryGetProperty("hidden", out var hiddenElement) && hiddenElement.ValueKind == JsonValueKind.True,
            InputMode = ParseInputMode(CliUtilities.ReadString(extensionElement, "inputMode")),
            DefaultJsonBody = CliUtilities.ReadString(extensionElement, "defaultJsonBody"),
        };

        foreach (var alias in CliUtilities.ReadStringArray(extensionElement, "aliases"))
        {
            metadata.Aliases.Add(alias);
        }

        foreach (var property in CliUtilities.ReadStringArray(extensionElement, "secretProperties"))
        {
            metadata.SecretProperties.Add(property);
        }

        if (extensionElement.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var argumentElement in argumentsElement.EnumerateArray())
            {
                var parameterName = CliUtilities.ReadString(argumentElement, "parameterName");
                var position = CliUtilities.ReadInt32(argumentElement, "position");
                if (!string.IsNullOrWhiteSpace(parameterName) && position.HasValue)
                {
                    metadata.Arguments.Add(new CliArgumentMetadata(parameterName, position.Value));
                }
            }
        }

        if (extensionElement.TryGetProperty("tableColumns", out var columnsElement) && columnsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var columnElement in columnsElement.EnumerateArray())
            {
                var propertyPath = CliUtilities.ReadString(columnElement, "propertyPath");
                var heading = CliUtilities.ReadString(columnElement, "heading");
                if (!string.IsNullOrWhiteSpace(propertyPath) && !string.IsNullOrWhiteSpace(heading))
                {
                    metadata.TableColumns.Add(new CliTableColumnMetadata(propertyPath, heading));
                }
            }
        }

        return true;
    }

    private static void ParseParameters(OpenApiOperationDefinition operation, JsonElement operationElement, JsonElement root)
    {
        if (!operationElement.TryGetProperty("parameters", out var parametersElement) || parametersElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var positions = operation.CliMetadata.Arguments.ToDictionary(argument => argument.ParameterName, argument => argument.Position, StringComparer.OrdinalIgnoreCase);

        foreach (var parameterElement in parametersElement.EnumerateArray())
        {
            var resolved = ResolveReferences(parameterElement, root);
            var schema = resolved.TryGetProperty("schema", out var schemaElement)
                ? ResolveReferences(schemaElement, root)
                : default;

            var parameter = new OpenApiParameterDefinition
            {
                Name = CliUtilities.ReadRequiredString(resolved, "name"),
                Location = CliUtilities.ReadRequiredString(resolved, "in"),
                Type = schema.ValueKind == JsonValueKind.Object ? ReadSchemaTypes(schema).FirstOrDefault(type => type != "null") ?? "string" : "string",
                Required = resolved.TryGetProperty("required", out var requiredElement) && requiredElement.ValueKind == JsonValueKind.True,
                Description = CliUtilities.ReadString(resolved, "description"),
            };

            if (positions.TryGetValue(parameter.Name, out var position))
            {
                parameter.ArgumentPosition = position;
            }

            operation.Parameters.RemoveAll(existing => existing.Name == parameter.Name && existing.Location == parameter.Location);
            operation.Parameters.Add(parameter);
        }

        operation.Parameters.Sort(static (left, right) => Nullable.Compare(left.ArgumentPosition, right.ArgumentPosition));
    }

    private static void ParseRequestBody(OpenApiOperationDefinition operation, JsonElement operationElement, JsonElement root)
    {
        if (!operationElement.TryGetProperty("requestBody", out var requestBodyElement))
        {
            return;
        }

        var resolvedBody = ResolveReferences(requestBodyElement, root);
        operation.RequestBodyRequired = resolvedBody.TryGetProperty("required", out var bodyRequiredElement)
            && bodyRequiredElement.ValueKind == JsonValueKind.True;

        if (!resolvedBody.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        JsonElement bodySchema = default;
        string? contentType = null;

        if (contentElement.TryGetProperty("application/json", out var jsonContent))
        {
            contentType = "application/json";
            bodySchema = ResolveSchema(jsonContent, root);
            operation.HasJsonRequestBody = true;
        }
        else if (contentElement.TryGetProperty("application/octet-stream", out var binaryContent))
        {
            contentType = "application/octet-stream";
            bodySchema = ResolveSchema(binaryContent, root);
            operation.HasBinaryRequestBody = true;
        }
        else
        {
            var firstContent = contentElement.EnumerateObject().FirstOrDefault();
            if (!string.IsNullOrEmpty(firstContent.Name))
            {
                contentType = firstContent.Name;
                bodySchema = ResolveSchema(firstContent.Value, root);
                operation.HasJsonRequestBody = !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);
                operation.HasBinaryRequestBody = string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);
            }
        }

        operation.RequestContentType = contentType;
        if (bodySchema.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        operation.RequestBodySchema = CreateStandaloneSchema(bodySchema, root);

        if (!ReadSchemaTypes(bodySchema).Contains("object"))
        {
            return;
        }

        var required = CliUtilities.ReadStringArray(bodySchema, "required").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!bodySchema.TryGetProperty("properties", out var propertiesElement) || propertiesElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in propertiesElement.EnumerateObject())
        {
            var resolvedProperty = ResolveReferences(property.Value, root);
            var definition = new RequestBodyPropertyDefinition
            {
                Name = property.Name,
                Type = ReadSchemaTypes(resolvedProperty).FirstOrDefault(type => type != "null"),
                AllowedTypes = ReadSchemaTypes(resolvedProperty),
                Required = required.Contains(property.Name),
                Description = CliUtilities.ReadString(resolvedProperty, "description"),
                MinimumLength = ReadInt32(resolvedProperty, "minLength"),
                MaximumLength = ReadInt32(resolvedProperty, "maxLength"),
                Minimum = ReadDouble(resolvedProperty, "minimum"),
                Maximum = ReadDouble(resolvedProperty, "maximum"),
            };

            if (resolvedProperty.TryGetProperty("enum", out var enumElement) && enumElement.ValueKind == JsonValueKind.Array)
            {
                definition.AllowedValues.AddRange(enumElement.EnumerateArray().Select(value => value.GetRawText()));
            }

            operation.RequestBodyProperties.Add(definition);
        }
    }

    private static JsonObject CreateStandaloneSchema(JsonElement schemaElement, JsonElement root)
    {
        var schema = JsonNode.Parse(schemaElement.GetRawText()) as JsonObject
            ?? throw new JsonException("The OpenAPI request body schema must be a JSON object.");
        var definitions = new JsonObject();
        var pending = new Queue<string>();
        var discovered = new HashSet<string>(StringComparer.Ordinal);

        CollectComponentReferences(schema, pending);

        var componentSchemas = root.TryGetProperty("components", out var components)
            && components.ValueKind == JsonValueKind.Object
            && components.TryGetProperty("schemas", out var schemas)
            && schemas.ValueKind == JsonValueKind.Object
                ? schemas
                : default;

        while (pending.TryDequeue(out var name))
        {
            if (!discovered.Add(name) || componentSchemas.ValueKind != JsonValueKind.Object || !componentSchemas.TryGetProperty(name, out var definition))
            {
                continue;
            }

            var definitionNode = JsonNode.Parse(definition.GetRawText())
                ?? throw new JsonException($"The OpenAPI schema component '{name}' is empty.");
            definitions[name] = definitionNode;
            CollectComponentReferences(definitionNode, pending);
        }

        RewriteComponentReferences(schema);
        RewriteComponentReferences(definitions);

        var dialect = root.TryGetProperty("jsonSchemaDialect", out var dialectElement) && dialectElement.ValueKind == JsonValueKind.String
            ? dialectElement.GetString()
            : "https://json-schema.org/draft/2020-12/schema";

        schema.TryAdd("$schema", dialect);

        if (definitions.Count > 0)
        {
            if (schema["$defs"] is JsonObject existingDefinitions)
            {
                foreach (var definition in definitions)
                {
                    existingDefinitions.TryAdd(definition.Key, definition.Value?.DeepClone());
                }
            }
            else
            {
                schema["$defs"] = definitions;
            }
        }

        return schema;
    }

    private static void CollectComponentReferences(JsonNode node, Queue<string> references)
    {
        if (node is JsonObject jsonObject)
        {
            if (jsonObject["$ref"]?.GetValue<string>() is { } reference
                && reference.StartsWith("#/components/schemas/", StringComparison.Ordinal))
            {
                references.Enqueue(UnescapeJsonPointerSegment(reference["#/components/schemas/".Length..]));
            }

            foreach (var property in jsonObject)
            {
                if (property.Value is not null)
                {
                    CollectComponentReferences(property.Value, references);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    CollectComponentReferences(item, references);
                }
            }
        }
    }

    private static void RewriteComponentReferences(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            if (jsonObject["$ref"]?.GetValue<string>() is { } reference
                && reference.StartsWith("#/components/schemas/", StringComparison.Ordinal))
            {
                jsonObject["$ref"] = $"#/$defs/{reference["#/components/schemas/".Length..]}";
            }

            foreach (var property in jsonObject)
            {
                if (property.Value is not null)
                {
                    RewriteComponentReferences(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    RewriteComponentReferences(item);
                }
            }
        }
    }

    private static string UnescapeJsonPointerSegment(string value) =>
        value.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);

    private static int? ReadInt32(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result) ? result : null;

    private static double? ReadDouble(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var result) ? result : null;

    private static JsonElement ResolveSchema(JsonElement contentElement, JsonElement root)
    {
        if (!contentElement.TryGetProperty("schema", out var schemaElement))
        {
            return default;
        }

        return ResolveReferences(schemaElement, root);
    }

    private static JsonElement ResolveReferences(JsonElement element, JsonElement root)
    {
        while (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("$ref", out var refElement) && refElement.ValueKind == JsonValueKind.String)
        {
            var reference = refElement.GetString();
            if (string.IsNullOrWhiteSpace(reference) || !reference.StartsWith("#/", StringComparison.Ordinal))
            {
                break;
            }

            element = ResolveJsonPointer(root, reference);
        }

        return element;
    }

    private static JsonElement ResolveJsonPointer(JsonElement root, string pointer)
    {
        var current = root;
        foreach (var rawSegment in pointer[2..].Split('/'))
        {
            var segment = rawSegment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
            current = current.GetProperty(segment);
        }

        return current;
    }

    private static CliInputMode ParseInputMode(string? value) => value?.ToLowerInvariant() switch
    {
        "json" => CliInputMode.Json,
        "stream" => CliInputMode.Stream,
        _ => CliInputMode.Options,
    };

    private static bool IsHttpMethod(string methodName) => methodName is "get" or "post" or "put" or "delete" or "patch" or "head";
}
