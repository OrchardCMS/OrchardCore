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

        public override async Task<IDisplayResult> DisplayAsync(ReCaptchaPart part, BuildPartDisplayContext context)
        {
            // TODO: We need an InitializeAsync so we can do this sort of initialization from within the shape factory..
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            return Initialize<ReCaptchaPartViewModel>("ReCaptchaPart", m =>
            {
                var settings = siteSettings.As<ReCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
                m.SiteKey = settings.SiteKey;
            }).Location("Detail", "Content");
        }

        public override async Task<IDisplayResult> EditAsync(ReCaptchaPart part, BuildPartEditorContext context)
        {
            // TODO: We need an InitializeAsync so we can do this sort of initialization from within the shape factory..
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            return Initialize<ReCaptchaPartEditViewModel>("ReCaptchaPart_Fields_Edit", m =>
            {
                var settings = siteSettings.As<ReCaptchaSettings>();
                m.SettingsAreConfigured = settings.IsValid();
            });
        }
    }
}
