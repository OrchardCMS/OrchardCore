using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public class RolesPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleTracker _roleTracker;

    public RolesPermissionHandler(IRoleTracker roleTracker)
    {
        _roleTracker = roleTracker;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || context?.User?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        var rolesWithFullAccess = await _roleTracker.GetAsync();

        if (rolesWithFullAccess.Count > 0)
        {
            foreach (var role in context.User.FindAll(c => c.Type is "role" or ClaimTypes.Role))
            {
                if (rolesWithFullAccess.Contains(role.Value))
                {
                    context.Succeed(requirement);

                    return;
                }
            }
        }
    }
}
