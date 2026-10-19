using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class PasskeyUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    public override IDisplayResult Display(UserMenu model, BuildDisplayContext context)
    {
        return View("UserMenuItems__Passkeys", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:16")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:16")
            .Differentiator("Passkeys");
    }
}
