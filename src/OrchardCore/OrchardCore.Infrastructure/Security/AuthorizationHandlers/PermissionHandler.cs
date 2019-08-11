using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.AuthorizationHandlers
{
    /// <summary>
    /// This authorization handler ensures that the user has the required permission.
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!(bool)context?.User?.Identity?.IsAuthenticated)
            {
                return Task.CompletedTask;
            }
            else if (context.User.HasClaim(Permission.ClaimType, requirement.Permission.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
