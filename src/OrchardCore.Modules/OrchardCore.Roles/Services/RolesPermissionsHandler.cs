using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Roles
{
    /// <summary>
    /// This authorization handler ensures that Anonymous and Authenticated permissions are checked.
    /// </summary>
    public class RolesPermissionsHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IPermissionGrantingService _permissionGrantingService;


        private IEnumerable<RoleClaim> _anonymousClaims = null, _authenticatedClaims = null;

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
            foreach (var claim in _anonymousClaims ??= await GetRoleClaimsAsync("Anonymous"))
            {
                claims.Add(claim);
            }

            if (context.User.Identity.IsAuthenticated)
            {
                foreach (var claim in _authenticatedClaims ??= await GetRoleClaimsAsync("Authenticated"))
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
            else
            {
                return Enumerable.Empty<RoleClaim>();
            }
        }
    }
}
