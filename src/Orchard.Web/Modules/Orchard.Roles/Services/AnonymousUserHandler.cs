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
    /// This authorization handler ensures that an anonymous user has its claims checked against
    /// the Anonymous role.
    /// </summary>
    [ScopedComponent(typeof(IAuthorizationHandler))]
    public class AnonymousUserHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRoleManager _roleManager;

        public AnonymousUserHandler(IRoleManager roleManager)
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
                var anonymous = await _roleManager.GetRoleByNameAsync("Anonymous");
                if (anonymous.RoleClaims.Any(x => x.ClaimType == Permission.ClaimType && requirement.Permission.Name == x.ClaimValue))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
