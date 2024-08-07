using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers;

public sealed class UserMenuNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("NavbarUserMenu", model)
            .Location("Detail", "Content:after")
            .Location("DetailAdmin", "Content:after");
    }
}
