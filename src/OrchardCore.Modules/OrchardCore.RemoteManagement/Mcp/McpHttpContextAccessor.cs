using Microsoft.AspNetCore.Http;

namespace OrchardCore.RemoteManagement.Mcp;

/// <summary>
/// Exposes the request dispatched by an MCP tool call to the services the endpoint uses, which
/// would otherwise see the outer MCP request, such as its response that may already have started.
/// </summary>
/// <remarks>
/// Assigning <see cref="HttpContextAccessor.HttpContext"/> can't do this: the setter clears the holder shared with the
/// calling flows, so that the MCP transport and the tenant's pipeline would read <see langword="null"/> after the call.
/// </remarks>
internal sealed class McpHttpContextAccessor : IHttpContextAccessor
{
    private static readonly AsyncLocal<ToolCall> s_toolCall = new();

    // Instances share the ambient context of the request, so this is the one set by the host.
    private readonly HttpContextAccessor _accessor = new();

    public HttpContext HttpContext
    {
        get => s_toolCall.Value?.Context ?? _accessor.HttpContext;
        set => _accessor.HttpContext = value;
    }

    /// <summary>
    /// Exposes the dispatched request until the returned scope is disposed.
    /// </summary>
    public static IDisposable BeginToolCall(HttpContext context)
    {
        var toolCall = new ToolCall { Context = context, Previous = s_toolCall.Value };
        s_toolCall.Value = toolCall;

        return toolCall;
    }

    private sealed class ToolCall : IDisposable
    {
        public HttpContext Context { get; set; }

        public ToolCall Previous { get; init; }

        public void Dispose()
        {
            // Clears the request for the flows started by the call that outlive it, which share this instance.
            Context = null;
            s_toolCall.Value = Previous;
        }
    }
}
