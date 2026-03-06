using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class UserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;

    public UserMenuDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ISiteService siteService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> DisplayAsync(UserMenu model, BuildDisplayContext context)
    {
        var results = new List<IDisplayResult>
        {
            View("UserMenuItems__Title", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Header:5")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Header:5")
            .Differentiator("Title"),

            View("UserMenuItems__SignedUser", model)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1")
            .Differentiator("SignedUser"),

            View("UserMenuItems__Profile", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:5")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:5")
            .Differentiator("Profile"),

            View("UserMenuItems__SignOut", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:100")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:100")
            .Differentiator("SignOut"),
        };

        var loginSettings = await _siteService.GetSettingsAsync<LoginSettings>();

        if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AdminPermissions.AccessAdminPanel))
        {
            results.Add(View("UserMenuItems__Dashboard", model)
                .Location(OrchardCoreConstants.DisplayType.Detail, "Content:1.1")
                .Differentiator("Dashboard"));

            if (!loginSettings.DisableLocalLogin)
            {
                results.Add(View("UserMenuItems__ChangePassword", model)
                    .Location(OrchardCoreConstants.DisplayType.Detail, "Content:10")
                    .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10")
                    .Differentiator("ChangePassword"));
            }
        }
        else
        {
            if (!loginSettings.DisableLocalLogin)
            {
                results.Add(View("UserMenuItems__ChangePassword", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10")
                .Differentiator("ChangePassword"));
            }
        }

        return Combine(results);
    }
}
