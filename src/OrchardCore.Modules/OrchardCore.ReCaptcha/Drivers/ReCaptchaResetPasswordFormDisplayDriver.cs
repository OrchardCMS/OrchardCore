using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaResetPasswordFormDisplayDriver : DisplayDriver<ResetPasswordForm>
{
    private readonly ISiteService _siteService;

    public ReCaptchaResetPasswordFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(ResetPasswordForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!settings.IsValid())
        {
            return null;
        }

        return View("FormReCaptcha", model).Location("Content:after");
    }
}
