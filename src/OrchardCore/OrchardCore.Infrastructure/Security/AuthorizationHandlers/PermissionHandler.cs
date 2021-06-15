using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.Security.AuthorizationHandlers
{
    /// <summary>
    /// This authorization handler ensures that the user has the required permission.
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionGrantingService _permissionGrantingService;

        public PermissionHandler(IPermissionGrantingService permissionGrantingService)
        {
            _permissionGrantingService = permissionGrantingService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded || !(context?.User?.Identity?.IsAuthenticated ?? false))
            {
                return Task.CompletedTask;
            }
            else if (_permissionGrantingService.IsGranted(requirement, context.User.Claims))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
