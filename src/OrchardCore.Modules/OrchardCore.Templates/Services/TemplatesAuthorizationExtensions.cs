using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.Templates.Services;

internal static class TemplatesAuthorizationExtensions
{
    /// <summary>
    /// Whether a user can manage the local templates and see the templates they override.
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    /// <param name="user">The user.</param>
    /// <param name="adminTemplates">Whether they are the admin templates.</param>
    public static async Task<bool> AuthorizeTemplatesAsync(this IAuthorizationService authorizationService, ClaimsPrincipal user, bool adminTemplates)
    {
        if (!await authorizationService.AuthorizeAsync(user, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return false;
        }

        var permission = adminTemplates
            ? AdminTemplatesPermissions.ManageAdminTemplates
            : Permissions.ManageTemplates;

        return await authorizationService.AuthorizeAsync(user, permission);
    }
}
