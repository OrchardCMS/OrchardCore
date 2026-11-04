using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels;

/// <summary>
/// Describes a public OAuth client that connects to the tenant's MCP endpoint.
/// </summary>
public sealed class RemoteManagementMcpViewModel
{
    /// <summary>
    /// Gets or sets the application's unique client identifier.
    /// </summary>
    [Required, StringLength(100)]
    public string ClientId { get; set; } = "orchardcore-mcp";

    /// <summary>
    /// Gets or sets one exact OAuth callback URI per line.
    /// </summary>
    [Required]
    public string RedirectUris { get; set; }

    /// <summary>
    /// Gets or sets whether this application's MCP security requirements are configured.
    /// </summary>
    public bool IsConfigured { get; set; }
}
