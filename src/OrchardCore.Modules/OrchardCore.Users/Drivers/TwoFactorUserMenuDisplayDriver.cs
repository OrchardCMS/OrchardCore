using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    public override Task<IDisplayResult> DisplayAsync(UserMenu model, BuildDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("UserMenuItems__TwoFactor", model)
            .Location("Detail", "Content:15")
            .Location("DetailAdmin", "Content:15")
            .Differentiator("TwoFactor")
        );
    }
}
