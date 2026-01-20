using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Roles;

/// <summary>
/// This authorization handler ensures that Anonymous and Authenticated permissions are checked.
/// </summary>
public class RolesPermissionsHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly RoleManager<IRole> _roleManager;
    private readonly IPermissionGrantingService _permissionGrantingService;


    private IEnumerable<RoleClaim> _anonymousClaims;
    private IEnumerable<RoleClaim> _authenticatedClaims;

    public RolesPermissionsHandler(
        RoleManager<IRole> roleManager,
        IPermissionGrantingService permissionGrantingService)
    {
        _roleManager = roleManager;
        _permissionGrantingService = permissionGrantingService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded)
        {
            // This handler is not revoking any pre-existing grants.
            return;
        }

        var claims = new HashSet<Claim>();
        foreach (var claim in _anonymousClaims ??= await GetRoleClaimsAsync(OrchardCoreConstants.Roles.Anonymous))
        {
            claims.Add(claim);
        }

        if (context.User.Identity.IsAuthenticated)
        {
            foreach (var claim in _authenticatedClaims ??= await GetRoleClaimsAsync(OrchardCoreConstants.Roles.Authenticated))
            {
                claims.Add(claim);
            }
        }

        if (_permissionGrantingService.IsGranted(requirement, claims))
        {
            context.Succeed(requirement);
            return;
        }
    }

    private async Task<IEnumerable<RoleClaim>> GetRoleClaimsAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role != null)
        {
            return ((Role)role).RoleClaims;
        }

        return [];
    }
}
