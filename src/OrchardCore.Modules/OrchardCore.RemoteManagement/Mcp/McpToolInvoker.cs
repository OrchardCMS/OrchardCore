using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using OrchardCore.Environment.Shell;

namespace OrchardCore.RemoteManagement.Mcp;

internal sealed class McpToolInvoker
{
    // Tool results are read by models, not rendered as HTML, so quotes stay readable.
    private static readonly JsonSerializerOptions s_resultSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly McpToolCatalog _catalog;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly McpToolOptions _options;
    private readonly ILogger<McpToolInvoker> _logger;

    public McpToolInvoker(McpToolCatalog catalog, IHttpContextAccessor httpContextAccessor, IOptions<McpToolOptions> options, ILogger<McpToolInvoker> logger)
    {
        _catalog = catalog;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
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

        return await InvokeEndpointAsync(tool, request.Arguments, parent, _logger, cancellationToken, _options.MaximumResponseBytes);
    }

    internal static async Task<CallToolResult> InvokeEndpointAsync(
        McpToolDefinition tool,
        IDictionary<string, JsonElement> arguments,
        HttpContext parent,
        ILogger logger,
        CancellationToken cancellationToken,
        int maximumResponseBytes = McpToolOptions.DefaultMaximumResponseBytes)
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
        context.MarkAsMcpToolCall();
        context.SetEndpoint(tool.Endpoint);
        context.Request.Method = tool.Method;
        context.Request.Scheme = parent.Request.Scheme;
        context.Request.Host = parent.Request.Host;
        context.Request.PathBase = parent.Request.PathBase;
        context.Request.Headers.Authorization = parent.Request.Headers.Authorization;

        // Services read these from the current request, such as to log the address of the caller or resolve the tenant's URLs.
        context.Connection.RemoteIpAddress = parent.Connection.RemoteIpAddress;
        context.Connection.RemotePort = parent.Connection.RemotePort;
        context.Connection.LocalIpAddress = parent.Connection.LocalIpAddress;
        context.Connection.LocalPort = parent.Connection.LocalPort;
        context.TraceIdentifier = parent.TraceIdentifier;
        context.Features.Set(parent.Features.Get<ShellContextFeature>());
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
        await using var output = new McpToolResponseStream(maximumResponseBytes);
        context.Request.Body = input;
        context.Request.ContentLength = input.Length;
        if (body is not null)
        {
            context.Request.ContentType = "application/json";
        }

        context.Features.Set<IHttpRequestBodyDetectionFeature>(new RequestBodyDetectionFeature(body is not null));
        context.Response.Body = output;
        using var toolCall = McpHttpContextAccessor.BeginToolCall(context);
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
            if (output.LimitExceeded)
            {
                return TooLargeError(maximumResponseBytes);
            }

            output.Position = 0;
            using var reader = new StreamReader(output, leaveOpen: true);
            var responseBody = await reader.ReadToEndAsync(cancellationToken);
            return new CallToolResult
            {
                IsError = context.Response.StatusCode >= StatusCodes.Status400BadRequest,
                Content = [new TextContentBlock { Text = FormatResult(context.Response.StatusCode, responseBody) }],
            };
        }
        catch (McpToolResponseTooLargeException)
        {
            return TooLargeError(maximumResponseBytes);
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

    private static string FormatResult(int statusCode, string body)
    {
        JsonNode content;
        try
        {
            content = string.IsNullOrEmpty(body) ? null : JsonNode.Parse(body);
        }
        catch (JsonException)
        {
            content = JsonValue.Create(body);
        }

        return new JsonObject { ["statusCode"] = statusCode, ["body"] = content }.ToJsonString(s_resultSerializerOptions);
    }

    private static CallToolResult TooLargeError(int maximumResponseBytes) =>
        Error($"The response is larger than the {maximumResponseBytes} bytes a tool can return, though a write operation may have been applied. Narrow the request, such as with paging, or use the HTTP API or Pomi.");

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
