using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
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
            var settings = await _siteService.GetSiteSettingsAsync();

            return Initialize<CaptchaPartViewModel>(m =>
            {
                m.SiteKey = settings.As<ReCaptchaSettings>().SiteKey;
            });
        }
    }
}
