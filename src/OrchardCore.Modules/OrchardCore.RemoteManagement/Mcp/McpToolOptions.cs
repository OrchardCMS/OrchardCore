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
}
