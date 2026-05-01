using System.Globalization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ReCaptchaSettings _settings;
    public ReCaptchaLoginFormDisplayDriver(IOptions<ReCaptchaSettings> options)
    {
        _settings = options.Value;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        if (!_settings.ConfigurationExists())
        {
            return null;
        }

        return Dynamic("ReCaptcha", (m) =>
        {
            m.language = CultureInfo.CurrentUICulture.Name;
        }).Location("Content:after");
    }
}
