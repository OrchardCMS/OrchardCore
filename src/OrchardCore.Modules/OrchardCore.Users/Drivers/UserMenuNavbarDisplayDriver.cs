using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers;

public class UserMenuNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model)
    {
        return View("NavbarUserMenu", model)
            .Location("Detail", "Content:after")
            .Location("DetailAdmin", "Content:after");
    }
}
