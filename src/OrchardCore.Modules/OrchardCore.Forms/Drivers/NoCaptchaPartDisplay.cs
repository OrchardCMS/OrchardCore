using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Forms.Configuration;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Forms.Drivers
{
    public class NoCaptchaPartDisplay : ContentPartDisplayDriver<NoCaptchaPart>
    {
        private readonly ISiteService _siteService;

        public NoCaptchaPartDisplay(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public override IDisplayResult Display(NoCaptchaPart part, BuildPartDisplayContext context)
        {
            return Initialize<NoCaptchaPartViewModel>("NoCaptchaPart", async m =>
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var settings = siteSettings.As<NoCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
                m.SiteKey = settings.SiteKey;
            }).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(NoCaptchaPart part, BuildPartEditorContext context)
        {
            return Initialize<NoCaptchaPartEditViewModel>("NoCaptchaPart_Fields_Edit", async m =>
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var settings = siteSettings.As<NoCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
            });
        }
    }
}
