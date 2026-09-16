using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace OrchardCore.RemoteManagement.Mcp;

internal sealed class McpToolInvoker
{
    private readonly McpToolCatalog _catalog;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<McpToolInvoker> _logger;

    public McpToolInvoker(McpToolCatalog catalog, IHttpContextAccessor httpContextAccessor, ILogger<McpToolInvoker> logger)
    {
        _catalog = catalog;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async ValueTask<CallToolResult> InvokeAsync(CallToolRequestParams request, CancellationToken cancellationToken)
    {
        var parent = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("An MCP HTTP request is required.");
        var tool = (await _catalog.GetToolsAsync(cancellationToken))
            .SingleOrDefault(tool => string.Equals(tool.Tool.Name, request.Name, StringComparison.Ordinal));
        if (tool is null)
        {
            return Error("Unknown or unavailable tool.");
        }

        return await InvokeEndpointAsync(tool, request.Arguments, parent, _logger, cancellationToken);
    }

    internal static async Task<CallToolResult> InvokeEndpointAsync(
        McpToolDefinition tool,
        IDictionary<string, JsonElement> arguments,
        HttpContext parent,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        arguments ??= new Dictionary<string, JsonElement>();
        var schema = tool.Tool.InputSchema;
        var properties = schema.GetProperty("properties");
        if (arguments.Keys.Any(key => !properties.TryGetProperty(key, out _)))
        {
            return Error("Unknown tool argument. Use the input schema returned by tools/list.");
        }

        foreach (var required in schema.GetProperty("required").EnumerateArray())
        {
            if (!arguments.ContainsKey(required.GetString()))
            {
                return Error($"Missing required argument: {required.GetString()}.");
            }
        }

        var context = new DefaultHttpContext
        {
            RequestServices = new McpRequestServices(parent),
            User = parent.User,
            RequestAborted = cancellationToken,
        };
        context.SetEndpoint(tool.Endpoint);
        context.Request.Method = tool.Method;
        context.Request.Scheme = parent.Request.Scheme;
        context.Request.Host = parent.Request.Host;
        context.Request.PathBase = parent.Request.PathBase;
        context.Request.Headers.Authorization = parent.Request.Headers.Authorization;
        var path = tool.Path;
        var query = new List<KeyValuePair<string, string>>();

        foreach (var location in new[] { "path", "query" })
        {
            if (!arguments.TryGetValue(location, out var values))
            {
                continue;
            }

            if (values.ValueKind != JsonValueKind.Object)
            {
                return Error($"The {location} argument must be an object.");
            }

            var group = properties.GetProperty(location);
            foreach (var required in group.GetProperty("required").EnumerateArray())
            {
                if (!values.TryGetProperty(required.GetString(), out _))
                {
                    return Error($"Missing required {location} parameter: {required.GetString()}.");
                }
            }

            foreach (var parameter in values.EnumerateObject())
            {
                if (!group.GetProperty("properties").TryGetProperty(parameter.Name, out _))
                {
                    return Error($"Unknown {location} parameter: {parameter.Name}.");
                }

                var items = parameter.Value.ValueKind == JsonValueKind.Array && location == "query"
                    ? parameter.Value.EnumerateArray().ToArray() : [parameter.Value];
                foreach (var item in items)
                {
                    if (item.ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False))
                    {
                        return Error($"The {location} parameter '{parameter.Name}' must contain scalar values.");
                    }

                    var value = item.ValueKind == JsonValueKind.String ? item.GetString() : item.GetRawText();
                    if (location == "path")
                    {
                        context.Request.RouteValues[parameter.Name] = value;
                        path = path.Replace("{" + parameter.Name + "}", Uri.EscapeDataString(value), StringComparison.Ordinal);
                    }
                    else
                    {
                        query.Add(new KeyValuePair<string, string>(parameter.Name, value));
                    }
                }
            }
        }

        context.Request.Path = path;
        context.Request.QueryString = QueryString.Create(query);
        var body = arguments.TryGetValue("body", out var bodyArgument) ? bodyArgument.GetRawText() : tool.Metadata.DefaultJsonBody;
        await using var input = new MemoryStream(body is null ? [] : Encoding.UTF8.GetBytes(body));
        await using var output = new MemoryStream();
        context.Request.Body = input;
        context.Request.ContentLength = input.Length;
        if (body is not null)
        {
            context.Request.ContentType = "application/json";
        }

        context.Features.Set<IHttpRequestBodyDetectionFeature>(new RequestBodyDetectionFeature(body is not null));
        context.Response.Body = output;
        try
        {
            // Run ASP.NET Core's authorization middleware against the original endpoint metadata.
            // Endpoint filters, binding, and the handler's resource-specific permission checks
            // execute in the original request delegate, using this tenant's request services.
            var authorization = new AuthorizationMiddleware(
                tool.Endpoint.RequestDelegate,
                context.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>());
            await authorization.Invoke(context);
            await context.Response.CompleteAsync();
            output.Position = 0;
            using var reader = new StreamReader(output, leaveOpen: true);
            var responseBody = await reader.ReadToEndAsync(cancellationToken);
            return new CallToolResult
            {
                IsError = context.Response.StatusCode >= StatusCodes.Status400BadRequest,
                Content = [new TextContentBlock { Text = JsonSerializer.Serialize(new { statusCode = context.Response.StatusCode, body = responseBody }) }],
            };
        }
        catch (BadHttpRequestException exception)
        {
            return Error($"The API rejected the request (HTTP {exception.StatusCode}). Check the tool input schema.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "MCP tool {ToolName} failed.", tool.Tool.Name);
            return Error("The management operation failed. See the tenant logs for details.");
        }
    }

    private static CallToolResult Error(string message) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = message }],
    };

    private sealed class RequestBodyDetectionFeature : IHttpRequestBodyDetectionFeature
    {
        public RequestBodyDetectionFeature(bool canHaveBody)
        {
            CanHaveBody = canHaveBody;
        }

        public bool CanHaveBody { get; }
    }
}
