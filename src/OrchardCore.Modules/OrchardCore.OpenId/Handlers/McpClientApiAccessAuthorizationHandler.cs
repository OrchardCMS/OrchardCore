using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.OpenId.Handlers;

/// <summary>
/// Keeps the tokens of automatically registered MCP clients on <c>/mcp</c>: anyone can register such a client, so
/// its token must not be usable on the other APIs, even with the consent of a user holding their permissions. The
/// operations invoked by a tool of <c>/mcp</c> are allowed. Other applications are unaffected, and an administrator
/// granted them their access.
/// </summary>
public sealed class McpClientApiAccessAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly object s_itemKey = new();

    private readonly IOpenIdApplicationManager _applications;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initializes the McpClientApiAccessAuthorizationHandler service.</summary>
    public McpClientApiAccessAuthorizationHandler(IOpenIdApplicationManager applications, IHttpContextAccessor httpContextAccessor)
    {
        _applications = applications;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.IsMcpToolCall() || httpContext.Request.Path.StartsWithSegments("/mcp"))
        {
            return;
        }

        var clientId = McpClientRequestValidation.GetClientId(context.User);
        if (string.IsNullOrEmpty(clientId))
        {
            return;
        }

        // A request can check many permissions, such as one per item of a list.
        if (httpContext.Items[s_itemKey] is not bool isDynamicClient)
        {
            isDynamicClient = await McpClientRequestValidation.IsDynamicClientAsync(_applications, clientId, httpContext.RequestAborted);
            httpContext.Items[s_itemKey] = isDynamicClient;
        }

        if (isDynamicClient)
        {
            context.Fail();
        }
    }
}
