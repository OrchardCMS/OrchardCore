using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Forms;

public sealed class ReCaptchaPartDisplayDriver : ContentPartDisplayDriver<ReCaptchaPart>
{
    private readonly ISiteService _siteService;

    public ReCaptchaPartDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override IDisplayResult Display(ReCaptchaPart part, BuildPartDisplayContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart", async model =>
        {
            var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();
            model.SettingsAreConfigured = settings.IsValid();
        }).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(ReCaptchaPart part, BuildPartEditorContext context)
    {
        return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart_Fields_Edit", async model =>
        {
            var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();
            model.SettingsAreConfigured = settings.IsValid();
        });
    }
}
