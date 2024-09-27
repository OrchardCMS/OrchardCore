using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Core;

public class RolesPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleService _roleService;
    private readonly IdentityOptions _identityOptions;

    public RolesPermissionHandler(
        IRoleService roleService,
        IOptions<IdentityOptions> identityOptions)
    {
        _roleService = roleService;
        _identityOptions = identityOptions.Value;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || context?.User?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        foreach (var claim in context.User.FindAll(IsRoleClaim))
        {
            if (await _roleService.IsAdminRoleAsync(claim.Value))
            {
                context.Succeed(requirement);

                return;
            }
        }
    }

    private bool IsRoleClaim(Claim claim)
        => claim.Type == _identityOptions.ClaimsIdentity.RoleClaimType ||
            (claim.Type is "role" or ClaimTypes.Role);
}
