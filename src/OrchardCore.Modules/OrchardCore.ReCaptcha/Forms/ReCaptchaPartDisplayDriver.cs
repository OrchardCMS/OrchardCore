using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Forms;

public sealed class ReCaptchaPartDisplayDriver : ContentPartDisplayDriver<ReCaptchaPart>
{
    private readonly IOptionsMonitor<ReCaptchaSettings> _settings;

    public ReCaptchaPartDisplayDriver(IOptionsMonitor<ReCaptchaSettings> options)
    {
        _settings = options;
    }

    public override IDisplayResult Display(ReCaptchaPart part, BuildPartDisplayContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart", async model =>
        {
            model.SettingsAreConfigured = _settings.CurrentValue.ConfigurationExists();
        }).Location(OrchardCoreConstants.DisplayType.Detail, "Content");
    }

    public override IDisplayResult Edit(ReCaptchaPart part, BuildPartEditorContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart_Fields_Edit", async model =>
        {
            model.SettingsAreConfigured = _settings.CurrentValue.ConfigurationExists();
        });
    }
}
