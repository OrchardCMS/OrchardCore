using Microsoft.AspNetCore.Authorization;
using Orchard.DependencyInjection;
using Orchard.Security.Permissions;

namespace Orchard.Security.AuthorizationHandlers
{
    /// <summary>
    /// This authorization handler ensures that the user has the required permission.
    /// </summary>
    [ScopedComponent(typeof(IAuthorizationHandler))]
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override void Handle(AuthorizationContext context, PermissionRequirement requirement)
        {
            if (!(bool)context?.User?.Identity?.IsAuthenticated)
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
