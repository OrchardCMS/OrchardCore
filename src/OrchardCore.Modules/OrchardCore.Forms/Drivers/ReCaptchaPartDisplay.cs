using System.Threading.Tasks;
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
    public class ReCaptchaPartDisplay : ContentPartDisplayDriver<ReCaptchaPart>
    {
        private readonly ISiteService _siteService;

        public ReCaptchaPartDisplay(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public override IDisplayResult Display(ReCaptchaPart part, BuildPartDisplayContext context)
        {
            return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart", async m =>
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var settings = siteSettings.As<ReCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
                m.SiteKey = settings.SiteKey;
            }).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ReCaptchaPart part, BuildPartEditorContext context)
        {
            return Initialize<ReCaptchaPartEditViewModel>("ReCaptchaPart_Fields_Edit", async m =>
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var settings = siteSettings.As<ReCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
            });
        }
    }
}
