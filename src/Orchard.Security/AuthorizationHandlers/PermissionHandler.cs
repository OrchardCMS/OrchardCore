using Microsoft.AspNetCore.Authorization;
using Orchard.DependencyInjection;
using Orchard.Security.Permissions;

namespace Orchard.Security.AuthorizationHandlers
{
    [ScopedComponent(typeof(IAuthorizationHandler))]
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override void Handle(AuthorizationContext context, PermissionRequirement requirement)
        {
            if (context?.User?.Identity == null)
            {
                return;
            }
            else if (context.User.HasClaim(Permission.ClaimType, requirement.Permission.Name))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
