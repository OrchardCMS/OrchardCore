using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class UserButtonsDisplayDriver : DisplayDriver<User>
{
    public override IDisplayResult Edit(User model, BuildEditorContext context)
    {
        return Dynamic("UserSaveButtons_Edit").Location("Actions");
    }
}
