using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorMethodLoginSmsDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override IDisplayResult Edit(TwoFactorMethod model)
    {
        return View("SmsAuthenticatorValidation", model)
        .Location("Content")
        .OnGroup(TokenOptions.DefaultPhoneProvider);
    }
}
