using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Forms;

public sealed class ReCaptchaPartDisplayDriver : ContentPartDisplayDriver<ReCaptchaPart>
{
    private readonly ReCaptchaSettings _settings;

    public ReCaptchaPartDisplayDriver(IOptions<ReCaptchaSettings> options)
    {
        _settings = options.Value;
    }

    public override IDisplayResult Display(ReCaptchaPart part, BuildPartDisplayContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart", async model =>
        {
            model.SettingsAreConfigured = _settings.ConfigurationExists();
        }).Location(OrchardCoreConstants.DisplayType.Detail, "Content");
    }

    public override IDisplayResult Edit(ReCaptchaPart part, BuildPartEditorContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart_Fields_Edit", async model =>
        {
            model.SettingsAreConfigured = _settings.ConfigurationExists();
        });
    }
}
