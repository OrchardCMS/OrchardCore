using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenOptionsConfiguration : IConfigureOptions<MediaTokenOptions>
    {
        private readonly ISiteService _siteService;

        public MediaTokenOptionsConfiguration(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public void Configure(MediaTokenOptions options)
        {
            var mediaTokenSettings = _siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<MediaTokenSettings>();

            if (mediaTokenSettings.HashKey == null)
            {
                var siteSettings = _siteService.LoadSiteSettingsAsync().GetAwaiter().GetResult();

                var rng = RandomNumberGenerator.Create();

                mediaTokenSettings.HashKey = new byte[64];
                rng.GetBytes(mediaTokenSettings.HashKey);
                siteSettings.Put(mediaTokenSettings);

                _siteService.UpdateSiteSettingsAsync(siteSettings).GetAwaiter().GetResult();
            }

            options.HashKey = mediaTokenSettings.HashKey;
        }
    }
}
