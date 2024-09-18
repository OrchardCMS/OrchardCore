using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalAuthenticationUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    private readonly SignInManager<IUser> _signInManager;

    public ExternalAuthenticationUserMenuDisplayDriver(SignInManager<IUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public override IDisplayResult Display(UserMenu model, BuildDisplayContext context)
    {
        return View("UserMenuItems__ExternalLogins", model)
            .RenderWhen(async () => (await _signInManager.GetExternalAuthenticationSchemesAsync()).Any())
            .Location("Detail", "Content:10")
            .Location("DetailAdmin", "Content:10")
            .Differentiator("ExternalLogins");
    }
}
