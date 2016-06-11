using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Orchard.DependencyInjection;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles
{
    /// <summary>
    /// This authorization handler ensures that an authenticated user has its claims checked against
    /// the Authenticated role.
    /// </summary>
    [ScopedComponent(typeof(IAuthorizationHandler))]
    public class AuthenticatedUserHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRoleManager _roleManager;

        public AuthenticatedUserHandler(IRoleManager roleManager)
        {
            _roleManager = roleManager;
        }

        protected override void Handle(AuthorizationContext context, PermissionRequirement requirement)
        {
            throw new NotImplementedException();
        }

        protected override async Task HandleAsync(AuthorizationContext context, PermissionRequirement requirement)
        {
            if (!(bool)context?.User?.Identity?.IsAuthenticated)
            {
                // TODO: Cache roles in IRoleManager
                var authenticated = await _roleManager.GetRoleByNameAsync("Authenticated");
                if (authenticated.RoleClaims.Any(x => x.ClaimType == Permission.ClaimType && requirement.Permission.Name == x.ClaimValue))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
