using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.Security;

public class ViewUserAuditTrailEventsHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private IAuthorizationService _authorizationService;
    private UserManager<IUser> _userManager;

    public ViewUserAuditTrailEventsHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded ||
            context.Resource is not AuditTrailUserEvent userEvent ||
            (requirement.Permission.Name != Permissions.ViewOwnUserAuditTrailEvents.Name && 
                requirement.Permission.Name != Permissions.ViewUserAuditTrailEvents.Name))
        {
            return;
        }
        
        // Lazy load to prevent circular dependencies
        _authorizationService ??= _serviceProvider.GetRequiredService<IAuthorizationService>();
        _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();
        
        var permission = await _userManager.GetUserAsync(context.User) is User { UserId: { } userId } &&
            userId.EqualsOrdinalIgnoreCase(userEvent.UserId)
                ? Permissions.ViewOwnUserAuditTrailEvents
                : Permissions.ViewUserAuditTrailEvents; 
        
        if (await _authorizationService.AuthorizeAsync(context.User, permission))
        {
            context.Succeed(requirement);
        }
    }
}
