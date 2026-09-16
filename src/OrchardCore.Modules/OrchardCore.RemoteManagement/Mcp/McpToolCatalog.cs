using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;

namespace OrchardCore.RemoteManagement.Mcp;

internal sealed class McpToolCatalog
{
    private readonly McpEndpointRegistry _registry;
    private readonly IOpenApiDocumentProvider _documents;

    public McpToolCatalog(McpEndpointRegistry registry, [FromKeyedServices("v1")] IOpenApiDocumentProvider documents)
    {
        _registry = registry;
        _documents = documents;
    }

    public async Task<IReadOnlyList<McpToolDefinition>> GetToolsAsync(CancellationToken cancellationToken)
    {
        var document = await _documents.GetOpenApiDocumentAsync(cancellationToken);
        var json = JsonNode.Parse(await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, cancellationToken));
        return CreateTools(_registry.DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>(), json);
    }

    internal static IReadOnlyList<McpToolDefinition> CreateTools(IEnumerable<RouteEndpoint> endpoints, JsonNode document)
    {
        var tools = new List<McpToolDefinition>();
        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.GetMetadata<CliOperationMetadata>();
            if (metadata is null || metadata.Hidden || metadata.FileResponse || metadata.InputMode == CliInputMode.Stream)
            {
                continue;
            }

            var operationId = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
            var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
            if (operationId is null || methods?.Count != 1 || endpoint.RequestDelegate is null)
            {
                continue;
            }

            var method = methods[0];
            foreach (var path in document["paths"].AsObject())
            {
                var operation = path.Value[method.ToLowerInvariant()];
                if (operation?["operationId"]?.GetValue<string>() != operationId)
                {
                    continue;
                }

                // Only JSON request bodies are portable MCP tool inputs. Binary and multipart
                // operations remain available through the HTTP API and Pomi.
                var body = operation["requestBody"];
                if (body is not null && body["content"]?["application/json"] is null)
                {
                    continue;
                }

                if (operation["responses"] is JsonObject responses && responses.Any(response =>
                    response.Key.StartsWith('2') && response.Value?["content"] is JsonObject content &&
                    content.Any(mediaType => mediaType.Key != "application/json" &&
                        !mediaType.Key.EndsWith("+json", StringComparison.OrdinalIgnoreCase) &&
                        !mediaType.Key.StartsWith("text/", StringComparison.OrdinalIgnoreCase))))
                {
                    continue;
                }

                var parameters = (path.Value["parameters"]?.AsArray() ?? [])
                    .Concat(operation["parameters"]?.AsArray() ?? []).ToArray();
                if (parameters.Any(parameter => parameter["in"]?.GetValue<string>() is not ("path" or "query")))
                {
                    continue;
                }

                var properties = new JsonObject();
                var required = new JsonArray();
                foreach (var location in new[] { "path", "query" })
                {
                    var group = new JsonObject();
                    var groupRequired = new JsonArray();
                    foreach (var parameter in parameters.Where(parameter => parameter["in"]?.GetValue<string>() == location))
                    {
                        var name = parameter["name"].GetValue<string>();
                        group[name] = parameter["schema"]?.DeepClone() ?? new JsonObject { ["type"] = "string" };
                        if (parameter["description"] is { } description)
                        {
                            group[name]["description"] = description.DeepClone();
                        }

                        if (location == "path" || parameter["required"]?.GetValue<bool>() == true)
                        {
                            groupRequired.Add(name);
                        }
                    }

                    if (group.Count > 0)
                    {
                        properties[location] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = group,
                            ["required"] = groupRequired,
                            ["additionalProperties"] = false,
                        };
                        if (groupRequired.Count > 0)
                        {
                            required.Add(location);
                        }
                    }
                }

                if (body is not null)
                {
                    properties["body"] = body["content"]["application/json"]["schema"]?.DeepClone() ?? new JsonObject();
                    if (body["required"]?.GetValue<bool>() == true && metadata.DefaultJsonBody is null)
                    {
                        required.Add("body");
                    }
                }

                var schema = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = properties,
                    ["required"] = required,
                    ["additionalProperties"] = false,
                };
                var definitions = new JsonObject();
                CopyReferences(schema, document["components"]?["schemas"], definitions);
                if (definitions.Count > 0)
                {
                    schema["$defs"] = definitions;
                }

                RewriteReferences(schema);
                var readOnly = HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
                tools.Add(new McpToolDefinition
                {
                    Endpoint = endpoint,
                    Method = method,
                    Path = path.Key,
                    Metadata = metadata,
                    Tool = new Tool
                    {
                        Name = string.Join('_', metadata.CommandGroup.Append(metadata.Verb)),
                        Description = operation["description"]?.GetValue<string>() ?? operation["summary"]?.GetValue<string>() ?? operationId,
                        InputSchema = JsonSerializer.SerializeToElement(schema),
                        Annotations = new ToolAnnotations
                        {
                            ReadOnlyHint = readOnly,
                            DestructiveHint = !readOnly,
                            IdempotentHint = readOnly || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method),
                            OpenWorldHint = false,
                        },
                    },
                });
                break;
            }
        }

        // Ambiguous names must never dispatch to an arbitrary endpoint.
        return tools.GroupBy(tool => tool.Tool.Name, StringComparer.Ordinal)
            .Where(group => group.Count() == 1).Select(group => group.Single())
            .OrderBy(tool => tool.Tool.Name, StringComparer.Ordinal).ToArray();
    }

    private static void CopyReferences(JsonNode node, JsonNode source, JsonObject definitions)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) &&
                value.StartsWith("#/components/schemas/", StringComparison.Ordinal))
            {
                var name = value["#/components/schemas/".Length..].Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
                if (!definitions.ContainsKey(name) && source?[name] is { } definition)
                {
                    definitions[name] = definition.DeepClone();
                    CopyReferences(definitions[name], source, definitions);
                }
            }

            foreach (var child in obj)
            {
                CopyReferences(child.Value, source, definitions);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                CopyReferences(child, source, definitions);
            }
        }
    }

    private static void RewriteReferences(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) &&
                value.StartsWith("#/components/schemas/", StringComparison.Ordinal))
            {
                obj["$ref"] = "#/$defs/" + value["#/components/schemas/".Length..];
            }

            foreach (var child in obj.ToArray())
            {
                RewriteReferences(child.Value);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                RewriteReferences(child);
            }
        }
    }
}

internal sealed class McpToolDefinition
{
    public Tool Tool { get; init; }
    public RouteEndpoint Endpoint { get; init; }
    public string Method { get; init; }
    public string Path { get; init; }
    public CliOperationMetadata Metadata { get; init; }
}
