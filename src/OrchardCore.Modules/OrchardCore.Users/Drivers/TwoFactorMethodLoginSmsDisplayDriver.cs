using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorMethodLoginSmsDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override Task<IDisplayResult> EditAsync(TwoFactorMethod model, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("SmsAuthenticatorValidation", model)
            .Location("Content")
            .OnGroup(TokenOptions.DefaultPhoneProvider)
        );
    }
}
