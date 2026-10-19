using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserPasskeyDisplayDriver : DisplayDriver<User>
{
    public override IDisplayResult Display(User user, BuildDisplayContext context)
    {
        if (user.Passkeys.Count == 0)
        {
            return null;
        }

        return Initialize<SummaryAdminUserViewModel>("UserPasskeyActionsMenu", model => model.User = user)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:11");
    }
}
