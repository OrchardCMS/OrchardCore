using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.Security.AuthorizationHandlers
{
    /// <summary>
    /// This authorization handler ensures that the user has the required permission.
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded || !(context?.User?.Identity?.IsAuthenticated ?? false))
            {
                return Task.CompletedTask;
            }
            else if (requirement.Permission.IsGranted(context.User.Claims))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
