using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RolesPermissionsHandler(
        RoleManager<IRole> roleManager,
        IPermissionGrantingService permissionGrantingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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
        }
        else
        {
            var permissionService = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

            foreach (var impliedBy in await permissionService.GetImplyingPermissionsAsync(requirement.Permission.Name))
            {
                if (impliedBy == null)
                {
                    continue;
                }

                if (_permissionGrantingService.IsGranted(new PermissionRequirement(impliedBy), context.User.Claims))
                {
                    context.Succeed(requirement);
                    break;
                }
            }
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
