using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class RoleTwoFactorAuthenticationHandler : ITwoFactorAuthenticationHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IUser> _userManager;
    private readonly ISiteService _siteService;

    public RoleTwoFactorAuthenticationHandler(
        IHttpContextAccessor httpContextAccessor,
        UserManager<IUser> userManager,
        ISiteService siteService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _siteService = siteService;
    }

    public async Task<bool> IsRequiredAsync()
    {
        if (_httpContextAccessor.HttpContext?.User == null)
        {
            return false;
        }

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user == null)
        {
            return false;
        }

        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<RoleLoginSettings>();

        if (loginSettings.RequireTwoFactorAuthenticationForSpecificRoles && loginSettings.Roles != null)
        {
            foreach (var role in loginSettings.Roles)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
