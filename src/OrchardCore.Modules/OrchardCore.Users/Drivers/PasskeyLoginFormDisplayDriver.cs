using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class PasskeyLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    public override IDisplayResult Edit(LoginForm model, BuildEditorContext context)
    {
        return View("LoginFormPasskey", model)
            .Location("Footer");
    }
}
