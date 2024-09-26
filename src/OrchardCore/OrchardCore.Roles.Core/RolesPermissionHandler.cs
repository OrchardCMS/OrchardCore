using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public class RolesPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IdentityOptions _identityOptions;
    private readonly ShellSettings _shellSettings;

    public RolesPermissionHandler(
        ShellSettings shellSettings,
        IOptions<IdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
        _shellSettings = shellSettings;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || context?.User?.Identity?.IsAuthenticated == false)
        {
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(c => IsRoleClaim(c) && c.Value.Equals(_shellSettings.GetSystemAdminRoleName(), StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool IsRoleClaim(Claim claim)
        => claim.Type == _identityOptions.ClaimsIdentity.RoleClaimType ||
            (claim.Type is "role" or ClaimTypes.Role);
}
