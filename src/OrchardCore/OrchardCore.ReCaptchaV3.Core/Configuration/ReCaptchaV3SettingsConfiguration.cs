using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptchaV3.Configuration
{
    public class ReCaptchaV3SettingsConfiguration : IConfigureOptions<ReCaptchaV3Settings>
    {
        private readonly ISiteService _site;

        public ReCaptchaV3SettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(ReCaptchaV3Settings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<ReCaptchaV3Settings>();

            options.SiteKey = settings.SiteKey;
            options.SecretKey = settings.SecretKey;
            options.Threshold = settings.Threshold;
        }
    }
}
