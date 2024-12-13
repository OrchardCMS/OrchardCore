using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class UserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public UserMenuDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> DisplayAsync(UserMenu model, BuildDisplayContext context)
    {
        var results = new List<IDisplayResult>
        {
            View("UserMenuItems__Title", model)
            .Location("Detail", "Header:5")
            .Location("DetailAdmin", "Header:5")
            .Differentiator("Title"),

            View("UserMenuItems__SignedUser", model)
            .Location("DetailAdmin", "Content:1")
            .Differentiator("SignedUser"),

            View("UserMenuItems__Profile", model)
            .Location("Detail", "Content:5")
            .Location("DetailAdmin", "Content:5")
            .Differentiator("Profile"),

            View("UserMenuItems__SignOut", model)
            .Location("Detail", "Content:100")
            .Location("DetailAdmin", "Content:100")
            .Differentiator("SignOut"),
        };

        if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AdminPermissions.AccessAdminPanel))
        {
            results.Add(View("UserMenuItems__Dashboard", model)
                .Location("Detail", "Content:1.1")
                .Differentiator("Dashboard"));
        }

        return Combine(results);
    }
}
