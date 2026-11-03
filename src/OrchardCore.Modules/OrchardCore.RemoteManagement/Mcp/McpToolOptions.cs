namespace OrchardCore.RemoteManagement.Mcp;

/// <summary>
/// Controls which management operations a tenant exposes as MCP tools.
/// </summary>
public sealed class McpToolOptions
{
    /// <summary>
    /// Gets or sets whether the operations that Pomi asks its users to confirm, such as executing a recipe or
    /// deleting a tenant, are exposed as MCP tools. A model can invoke a tool without asking anyone, and
    /// annotations only advise clients to ask, so these operations stay off by default.
    /// </summary>
    public bool AllowConfirmationRequiredTools { get; set; }

    /// <summary>
    /// Gets or sets the largest response, in bytes, a tool can return. Responses are held in memory
    /// to become the text of the tool's result, so a larger one is turned into a tool error.
    /// </summary>
    public int MaximumResponseBytes { get; set; } = DefaultMaximumResponseBytes;

    internal const int DefaultMaximumResponseBytes = 4 * 1024 * 1024;
}
