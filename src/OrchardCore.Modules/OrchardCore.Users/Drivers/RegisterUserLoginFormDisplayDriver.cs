using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class RegisterUserLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    public override IDisplayResult Edit(LoginForm model, BuildEditorContext context)
    {
        return View("LoginFormRegisterUser", model).Location("Links:10");
    }
}
