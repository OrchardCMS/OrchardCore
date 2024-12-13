using System.Globalization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaForgotPasswordFormDisplayDriver : DisplayDriver<ForgotPasswordForm>
{
    private readonly ISiteService _siteService;

    public ReCaptchaForgotPasswordFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(ForgotPasswordForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!settings.ConfigurationExists())
        {
            return null;
        }

        return Dynamic("ReCaptcha", (m) =>
        {
            m.language = CultureInfo.CurrentUICulture.Name;
        }).Location("Content:after");
    }
}
