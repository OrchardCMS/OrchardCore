using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Configuration
{
    public class ReCaptchaSettingsConfiguration : IAsyncConfigureOptions<ReCaptchaSettings>
    {
        private readonly ISiteService _site;

        public ReCaptchaSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public async ValueTask ConfigureAsync(ReCaptchaSettings options)
        {
            var settings = (await _site.GetSiteSettingsAsync()).As<ReCaptchaSettings>();

            options.SiteKey = settings.SiteKey;
            options.SecretKey = settings.SecretKey;
        }
    }
}
