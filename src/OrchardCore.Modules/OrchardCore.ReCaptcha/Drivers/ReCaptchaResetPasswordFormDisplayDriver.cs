using System.Globalization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaResetPasswordFormDisplayDriver : DisplayDriver<ResetPasswordForm>
{
    private readonly IOptionsMonitor<ReCaptchaSettings> _settings;

    public ReCaptchaResetPasswordFormDisplayDriver(IOptionsMonitor<ReCaptchaSettings> options)
    {
        _settings = options;
    }

    public override async Task<IDisplayResult> EditAsync(ResetPasswordForm model, BuildEditorContext context)
    {
        if (!_settings.CurrentValue.ConfigurationExists())
        {
            return null;
        }

        return Dynamic("ReCaptcha", (m) =>
        {
            m.language = CultureInfo.CurrentUICulture.Name;
        }).Location("Content:after");
    }
}
