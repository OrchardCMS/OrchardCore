using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ISiteService _siteService;
    private readonly ReCaptchaService _reCaptchaService;

    public ReCaptchaLoginFormDisplayDriver(
        ISiteService siteService,
        ReCaptchaService reCaptchaService)
    {
        _siteService = siteService;
        _reCaptchaService = reCaptchaService;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        var _reCaptchaSettings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!_reCaptchaSettings.IsValid() || !_reCaptchaService.IsThisARobot())
        {
            return null;
        }

        return View("FormReCaptcha", model).Location("Content:after");
    }
}
