using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Forms.Configuration
{
    public class NoCaptchaSettingsConfiguration : IConfigureOptions<NoCaptchaSettings>
    {
        private readonly ISiteService _site;

        public NoCaptchaSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(NoCaptchaSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<NoCaptchaSettings>();

            options.SiteKey = settings.SiteKey;
            options.SiteSecret = settings.SiteSecret;
        }
    }
}
