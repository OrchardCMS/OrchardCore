using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserTwoFactorDisplayDriver : DisplayDriver<User>
{
    public override Task<IDisplayResult> DisplayAsync(User user, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<SummaryAdminUserViewModel>("UserTwoFactorActionsMenu", model => model.User = user)
            .Location("SummaryAdmin", "ActionsMenu:10")
        );
    }
}
