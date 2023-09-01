using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class UserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    private readonly SignInManager<IUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserMenuDisplayDriver(
        SignInManager<IUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(UserMenu model)
    {
        var results = new List<IDisplayResult>
        {
            View("UserMenuItems__Title", model)
            .Differentiator("Title")
            .Location("Detail", "Header:5")
            .Location("DetailAdmin", "Header:5"),

            View("UserMenuItems__SignedUser", model)
            .Differentiator("SignedUser")
            .Location("DetailAdmin", "Content:1"),

            View("UserMenuItems__Profile", model)
            .Differentiator("Profile")
            .Location("Detail", "Content:5")
            .Location("DetailAdmin", "Content:5"),

            View("UserMenuItems__ExternalLogins", model)
            .Differentiator("ExternalLogins")
            .RenderWhen(async () => (await _signInManager.GetExternalAuthenticationSchemesAsync()).Any())
            .Location("Detail", "Content:10")
            .Location("DetailAdmin", "Content:10"),

            View("UserMenuItems__SignOut", model)
            .Differentiator("SignOut")
            .Location("Detail", "Content:100")
            .Location("DetailAdmin", "Content:100"),
        };

        if (_httpContextAccessor.HttpContext.User.HasClaim("Permission", "AccessAdminPanel"))
        {
            results.Add(View("UserMenuItems__Dashboard", model)
                .Differentiator("Dashboard")
                .Location("Detail", "Content:1.1"));
        }

        return Combine(results);
    }
}
