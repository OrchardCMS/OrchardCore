using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorMethodLoginAuthenticationAppDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override IDisplayResult Edit(TwoFactorMethod model)
    {
        return View("AuthenticatorAppValidation", model)
        .Location("Content")
        .OnGroup(TokenOptions.DefaultAuthenticatorProvider);
    }
}
