using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public class RolesPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleTracker _roleTracker;
    private readonly IdentityOptions _identityOptions;

    public RolesPermissionHandler(
        IRoleTracker roleTracker,
        IOptions<IdentityOptions> identityOptions)
    {
        _roleTracker = roleTracker;
        _identityOptions = identityOptions.Value;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || context?.User?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        var rolesWithFullAccess = await _roleTracker.GetAsync();

        if (rolesWithFullAccess.Count == 0)
        {
            return;
        }

        foreach (var role in context.User.FindAll(c => c.Type == _identityOptions.ClaimsIdentity.RoleClaimType || c.Type is "role" or ClaimTypes.Role))
        {
            if (rolesWithFullAccess.Contains(role.Value))
            {
                context.Succeed(requirement);

                return;
            }
        }
    }
}
