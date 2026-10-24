using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class SecurityUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    public override IDisplayResult Display(UserMenu model, BuildDisplayContext context)
    {
        return View("UserMenuItems__Security", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:15")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:15")
            .Differentiator("Security");
    }
}
