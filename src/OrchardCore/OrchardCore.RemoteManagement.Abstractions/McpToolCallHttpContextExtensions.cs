using Microsoft.AspNetCore.Http;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Identifies the requests dispatched to a management endpoint by an MCP tool call, whose
/// arguments and results reach the model's context and transcript.
/// </summary>
public static class McpToolCallHttpContextExtensions
{
    private static readonly object s_itemKey = new();

    /// <summary>
    /// Marks the request as dispatched by an MCP tool call.
    /// </summary>
    /// <param name="context">The request dispatched to the endpoint.</param>
    public static void MarkAsMcpToolCall(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Items[s_itemKey] = true;
    }

    /// <summary>
    /// Gets whether the request was dispatched by an MCP tool call.
    /// </summary>
    /// <param name="context">The current request.</param>
    /// <returns><see langword="true"/> for an MCP tool call; otherwise, <see langword="false"/>.</returns>
    public static bool IsMcpToolCall(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.ContainsKey(s_itemKey);
    }
}
