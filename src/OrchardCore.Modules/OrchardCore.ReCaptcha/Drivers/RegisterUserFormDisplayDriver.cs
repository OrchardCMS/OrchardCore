using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public class RegisterUserFormDisplayDriver : DisplayDriver<RegisterUserForm>
{
    private readonly ISiteService _siteService;

    public RegisterUserFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(RegisterUserForm model, BuildEditorContext context)
    {
        var _reCaptchaSettings = (await _siteService.GetSiteSettingsAsync()).As<ReCaptchaSettings>();

        if (!_reCaptchaSettings.IsValid())
        {
            return null;
        }

        return View("FormReCaptcha", model).Location("Content:after");
    }
}
