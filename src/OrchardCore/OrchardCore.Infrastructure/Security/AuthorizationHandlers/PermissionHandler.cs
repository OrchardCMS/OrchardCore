using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.AuthorizationHandlers;

/// <summary>
/// This authorization handler ensures that the user has the required permission.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionGrantingService _permissionGrantingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(IPermissionGrantingService permissionGrantingService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionGrantingService = permissionGrantingService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || !(context?.User?.Identity?.IsAuthenticated ?? false))
        {
            return;
        }

        if (_permissionGrantingService.IsGranted(requirement, context.User.Claims))
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
}
