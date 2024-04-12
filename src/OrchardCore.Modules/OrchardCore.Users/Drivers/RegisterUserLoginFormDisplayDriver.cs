using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class RegisterUserLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ISiteService _siteService;

    public RegisterUserLoginFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

        if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
        {
            return null;
        }

        return View("LoginFormRegisterUser_Edit", model).Location("Links:10");
    }
}
