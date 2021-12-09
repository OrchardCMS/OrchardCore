using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Configuration
{
    public class ReCaptchaSettingsConfiguration : IConfigureOptions<ReCaptchaSettings>
    {
        private readonly ISiteService _site;

        public ReCaptchaSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(ReCaptchaSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<ReCaptchaSettings>();

            options.SiteKey = settings.SiteKey;
            options.SecretKey = settings.SecretKey;
        }
    }
}
