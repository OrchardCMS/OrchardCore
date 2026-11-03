using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId.Handlers;

/// <summary>
/// Checks the audience of the tokens of automatically registered clients, which <see cref="McpTokenResourceHandler"/>
/// sets to the MCP URL of the tenant host they were requested for. Other applications have no MCP audience: an
/// administrator granted them their access, and they can call every management API.
/// </summary>
public sealed class McpResourceAudienceAuthorizationHandler : AuthorizationHandler<McpResourceAudienceRequirement, HttpContext>
{
    private readonly IOpenIdApplicationManager _applications;

    /// <summary>Initializes the McpResourceAudienceAuthorizationHandler service.</summary>
    public McpResourceAudienceAuthorizationHandler(IOpenIdApplicationManager applications)
    {
        _applications = applications;
    }

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, McpResourceAudienceRequirement requirement, HttpContext resource)
    {
        var clientId = context.User.GetPresenters().FirstOrDefault() ?? context.User.GetClaim(OpenIddictConstants.Claims.ClientId);
        if (!await McpClientRequestValidation.IsDynamicClientAsync(_applications, clientId, resource.RequestAborted))
        {
            context.Succeed(requirement);

            return;
        }

        var request = resource.Request;
        var audience = McpClientRequestValidation.GetTenantEndpoint(new Uri($"{request.Scheme}://{request.Host}{request.PathBase}"), "mcp");
        if (context.User.GetAudiences().Contains(audience, StringComparer.Ordinal))
        {
            context.Succeed(requirement);
        }
    }
}
