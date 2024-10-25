using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security;

namespace OrchardCore.Settings.Services;

/// <summary>
/// This authorization handler validates any permission when the user is the site owner.
/// </summary>
public class SuperUserHandler : IAuthorizationHandler
{
    private readonly ISiteService _siteService;

    public SuperUserHandler(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var user = context?.User;

        if (user == null)
        {
            return;
        }

        if (user.HasClaim(StandardClaims.SiteOwner.Type, StandardClaims.SiteOwner.Value))
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
