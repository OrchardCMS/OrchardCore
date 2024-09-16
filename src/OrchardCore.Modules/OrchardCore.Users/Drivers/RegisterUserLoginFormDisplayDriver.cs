using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class RegisterUserLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly RegistrationOptions _registrationOptions;

    public RegisterUserLoginFormDisplayDriver(IOptions<RegistrationOptions> registrationOptions)
    {
        _registrationOptions = registrationOptions.Value;
    }

    public override IDisplayResult Edit(LoginForm model, BuildEditorContext context)
    {
        if (!_registrationOptions.AllowSiteRegistration)
        {
            return null;
        }

        return View("LoginFormRegisterUser", model).Location("Links:10");
    }
}
