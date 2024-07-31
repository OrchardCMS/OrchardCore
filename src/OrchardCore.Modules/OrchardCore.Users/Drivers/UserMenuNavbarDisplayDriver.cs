using System.Threading.Tasks;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers;

public class UserMenuNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("NavbarUserMenu", model)
            .Location("Detail", "Content:after")
            .Location("DetailAdmin", "Content:after")
        );
    }
}
