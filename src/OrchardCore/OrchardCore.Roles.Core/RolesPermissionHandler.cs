using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Core;

public class RolesPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IdentityOptions _identityOptions;
    private readonly IRoleService _roleService;

    private string[] _rolesWithOwnerType;

    public RolesPermissionHandler(
        IRoleService roleService,
        IOptions<IdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || context?.User?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        _rolesWithOwnerType ??= (await _roleService.GetRolesAsync())
            .Where(x => x.Type.HasFlag(RoleType.Owner))
            .Select(x => x.RoleName)
            .ToArray();

        if (_rolesWithOwnerType.Length == 0)
        {
            return;
        }

        foreach (var role in context.User.FindAll(c => c.Type == _identityOptions.ClaimsIdentity.RoleClaimType || c.Type is "role" or ClaimTypes.Role))
        {
            if (_rolesWithOwnerType.Contains(role.Value, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);

                return;
            }
        }
    }
}
