using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Roles;
using OrchardCore.Security;

namespace OrchardCore.Settings.Services;

/// <summary>
/// This authorization handler validates any permission when the user is the site owner.
/// </summary>
public class SuperUserHandler : IAuthorizationHandler
{
    private readonly ISiteService _siteService;
    private readonly ISystemRoleProvider _systemRoleProvider;

    public SuperUserHandler(
        ISiteService siteService,
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable IDE0060 // Remove unused parameter
        ISystemRoleNameProvider systemRoleNameProvider,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS0618 // Type or member is obsolete
        ISystemRoleProvider systemRoleProvider)
    {
        _siteService = siteService;
        _systemRoleProvider = systemRoleProvider;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var user = context?.User;

        if (user == null)
        {
            return;
        }

        var adminRole = await _systemRoleProvider.GetAdminRoleAsync();

        if (user.IsInRole(adminRole.RoleName))
        {
            SucceedAllRequirements(context);

            return;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        if (string.Equals(userId, site.SuperUser, StringComparison.OrdinalIgnoreCase))
        {
            SucceedAllRequirements(context);
        }
    }

    private static void SucceedAllRequirements(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.Requirements.OfType<PermissionRequirement>())
        {
            context.Succeed(requirement);
        }
    }
}
