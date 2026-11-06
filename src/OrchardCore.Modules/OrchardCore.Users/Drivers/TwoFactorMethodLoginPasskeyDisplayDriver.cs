using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class TwoFactorMethodLoginPasskeyDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override IDisplayResult Edit(TwoFactorMethod model, BuildEditorContext context)
    {
        return View("PasskeyValidation", model)
            .Location("Content")
            .OnGroup(UserConstants.PasskeyTwoFactorProvider);
    }
}
