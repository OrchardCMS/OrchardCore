using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class RegisterUserFormDisplayDriver : DisplayDriver<RegisterUserForm>
{
    private readonly ISiteService _siteService;

    public RegisterUserFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(RegisterUserForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!settings.IsValid())
        {
            return null;
        }

        return View("FormReCaptcha", model).Location("Content:after");
    }
}
