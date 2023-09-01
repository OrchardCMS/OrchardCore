using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    public override IDisplayResult Display(UserMenu model)
    {
        return View("UserMenuItems__TwoFactor", model)
            .Differentiator("TwoFactor")
            .Location("Detail", "Content:15")
            .Location("DetailAdmin", "Content:15");
    }
}
