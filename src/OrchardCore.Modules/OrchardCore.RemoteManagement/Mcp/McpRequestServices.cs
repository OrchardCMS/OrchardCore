using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.RemoteManagement.Mcp;

// Reuse the tenant's scoped services while keeping authentication results local to the
// synthetic API response. Authentication handlers are cached per HTTP request and hold
// the outer MCP HttpContext, whose streaming response may already have started.
internal sealed class McpRequestServices : IServiceProvider, IKeyedServiceProvider, IAuthenticationService
{
    private readonly HttpContext _parent;

    public McpRequestServices(HttpContext parent)
    {
        _parent = parent;
    }

    public object GetService(Type serviceType) => serviceType == typeof(IAuthenticationService)
        ? this
        : serviceType == typeof(IServiceProvider) ? this : _parent.RequestServices.GetService(serviceType);

    public object GetKeyedService(Type serviceType, object serviceKey) =>
        (_parent.RequestServices as IKeyedServiceProvider)?.GetKeyedService(serviceType, serviceKey);

    public object GetRequiredKeyedService(Type serviceType, object serviceKey) =>
        ((IKeyedServiceProvider)_parent.RequestServices).GetRequiredKeyedService(serviceType, serviceKey);

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
    {
        // The MCP endpoint has already validated this API bearer identity. A tool cannot
        // switch to cookies or another authentication scheme through its endpoint policy.
        var result = (scheme is null || scheme == OrchardCoreConstants.AuthenticationSchemes.Api) &&
            _parent.User.Identity?.IsAuthenticated == true
            ? AuthenticateResult.Success(new AuthenticationTicket(_parent.User, OrchardCoreConstants.AuthenticationSchemes.Api))
            : AuthenticateResult.NoResult();
        return Task.FromResult(result);
    }

    public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public Task SignInAsync(HttpContext context, string scheme, System.Security.Claims.ClaimsPrincipal principal, AuthenticationProperties properties) =>
        throw new NotSupportedException("MCP tools cannot sign in users.");

    public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties) =>
        throw new NotSupportedException("MCP tools cannot sign out users.");
}
