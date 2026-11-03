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
    private readonly IRoleStore<IRole> _roleStore;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IPermissionGrantingService _permissionGrantingService;


    // The claims of the anonymous role, and of the anonymous and authenticated roles, built once per scope
    // instead of for each authorization check.
    private Claim[] _anonymousClaims;
    private Claim[] _authenticatedClaims;

    public RolesPermissionsHandler(
        IRoleStore<IRole> roleStore,
        ILookupNormalizer lookupNormalizer,
        IPermissionGrantingService permissionGrantingService)
    {
        // The role store is used directly, as the role manager, its validators and all their dependencies
        // would otherwise be built on each request only to look up a role by its normalized name.
        _roleStore = roleStore;
        _lookupNormalizer = lookupNormalizer;
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
        var role = await _roleStore.FindByNameAsync(_lookupNormalizer.NormalizeName(roleName), CancellationToken.None);

        if (role != null)
        {
            return ((Role)role).RoleClaims;
        }

        return [];
    }
}
