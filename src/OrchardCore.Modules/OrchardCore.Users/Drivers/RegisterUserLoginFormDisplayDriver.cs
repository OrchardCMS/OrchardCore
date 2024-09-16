using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class RegisterUserLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ISiteService _siteService;

    public RegisterUserLoginFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<RegistrationSettings>();

        if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
        {
            return null;
        }

        return View("LoginFormRegisterUser", model).Location("Links:10");
    }
}
