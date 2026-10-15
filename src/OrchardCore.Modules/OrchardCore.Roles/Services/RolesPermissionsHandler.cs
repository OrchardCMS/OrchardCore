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


    // The claims of the anonymous role, and of the anonymous and authenticated roles, built once per scope
    // instead of for each authorization check.
    private Claim[] _anonymousClaims;
    private Claim[] _authenticatedClaims;

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

        _anonymousClaims ??= ToClaims(await GetRoleClaimsAsync(OrchardCoreConstants.Roles.Anonymous));

        var claims = _anonymousClaims;

        if (context.User.Identity.IsAuthenticated)
        {
            claims = _authenticatedClaims ??= [.. _anonymousClaims, .. ToClaims(await GetRoleClaimsAsync(OrchardCoreConstants.Roles.Authenticated))];
        }

        if (_permissionGrantingService.IsGranted(requirement, claims))
        {
            context.Succeed(requirement);
            return;
        }
    }

    private static Claim[] ToClaims(IEnumerable<RoleClaim> roleClaims)
        => roleClaims.Select(roleClaim => (Claim)roleClaim).ToArray();

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
