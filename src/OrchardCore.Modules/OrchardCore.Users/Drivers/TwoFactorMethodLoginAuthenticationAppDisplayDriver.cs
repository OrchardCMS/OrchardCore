using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class TwoFactorMethodLoginAuthenticationAppDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override IDisplayResult Edit(TwoFactorMethod model, BuildEditorContext context)
    {
        return View("AuthenticatorAppValidation", model)
            .Location("Content")
            .OnGroup(TokenOptions.DefaultAuthenticatorProvider);
    }
}
