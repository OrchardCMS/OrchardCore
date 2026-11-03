using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Requires an access token issued to an automatically registered MCP client to target the MCP resource of the current request.
/// Other callers are unaffected, such as client credentials applications and Pomi, whose tokens don't target a specific resource.
/// </summary>
public sealed class McpResourceAudienceRequirement : IAuthorizationRequirement
{
}
