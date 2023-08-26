using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public override async Task<IDisplayResult> DisplayAsync(UserMenu model, BuildDisplayContext context)
    {
        var results = new List<IDisplayResult>
        {
            View("UserMenuItems__Profile", model)
            .Location("Detail", "Content:5")
            .Location("DetailAdmin", "Content:5"),

            View("UserMenuItems__SignOut", model)
            .Location("Detail", "Content:after")
            .Location("DetailAdmin", "Content:after"),

            View("UserMenuItems__Title", model)
            .Location("Detail", "Header:5")
            .Location("DetailAdmin", "Header:5"),

            View("UserMenuItems__SignedUser", model)
            .Location("Detail", "Content:before")
            .Location("DetailAdmin", "Content:before"),

            View("UserMenuItems__ExternalLogins", model)
            .RenderWhen(async () => (await _signInManager.GetExternalAuthenticationSchemesAsync()).Any())
            .Location("Detail", "Content:10")
            .Location("DetailAdmin", "Content:10"),
        };

        if (_httpContextAccessor.HttpContext.User.HasClaim("Permission", "AccessAdminPanel"))
        {
            results.Add(View("UserMenuItems__Dashboard", model)
                .Location("Detail", "Content:1"));
        }

        return Combine(results);
    }
}
