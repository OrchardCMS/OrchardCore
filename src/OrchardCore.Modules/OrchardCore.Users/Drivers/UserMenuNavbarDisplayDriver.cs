using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers;

public sealed class UserMenuNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("NavbarUserMenu", model)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:after")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:after");
    }
}
